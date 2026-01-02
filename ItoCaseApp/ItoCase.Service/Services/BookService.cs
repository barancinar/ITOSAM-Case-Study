using AutoMapper;
using ExcelDataReader;
using ItoCase.Core.DTOs;
using ItoCase.Core.Entities;
using ItoCase.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ItoCase.Service.Services
{
    public class BookService : IBookService
    {
        //private readonly IGenericRepository<Book> _bookRepository;
        // UnitOfWork pattern kullanımı için IUnitOfWork eklendi
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public BookService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<BookDto>> GetAllBooksAsync()
        {
            var books = await _unitOfWork.Books.GetAllAsync();
            // Entity -> DTO Dönüşümü
            return _mapper.Map<List<BookDto>>(books);
        }

        public async Task ImportExcelToDatabaseAsync(string filePath)
        {
            // Windows-1252 kodlaması için gerekli (Excel okurken Türkçe karakter sorunu olmaması için)
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            var booksToAdd = new List<Book>();

            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                        {
                            UseHeaderRow = true // İlk satır başlık olsun
                        }
                    });

                    var dataTable = result.Tables[0];

                    foreach (DataRow row in dataTable.Rows)
                    {
                        // Excel sütun isimleri Scraper/main.py dosyasındaki ile AYNI olmalı
                        // "Fiyat" sütunundaki "TL", nokta, virgül temizliği:
                        string rawPrice = row["Fiyat"]?.ToString()?.Replace("TL", "").Trim() ?? "0";

                        decimal finalPrice = 0;

                        if (decimal.TryParse(rawPrice, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal p))
                        {
                            finalPrice = p;
                        }

                        string rawSales = row["SatisRakamlari"]?.ToString() ?? "0";
                        int finalSales = 0;
                        if (int.TryParse(rawSales, out int s)) finalSales = s;

                        var book = new Book
                        {
                            AnaKategori = row["AnaKategori"]?.ToString(),
                            Turu = row["Turu"]?.ToString(),
                            KitapAdi = row["KitapAdi"]?.ToString(),
                            Yazar = row["Yazar"]?.ToString(),
                            Yayinevi = row["Yayinevi"]?.ToString(),
                            Fiyat = finalPrice,
                            ISBN = row["ISBN"]?.ToString(),
                            SayfaSayisi = row["SayfaSayisi"]?.ToString(),
                            BasimYili = row["BasimYili"]?.ToString(),
                            KagitTipi = row["KagitTipi"]?.ToString(),
                            KapakTipi = row["KapakTipi"]?.ToString(),
                            SatisRakamlari = finalSales
                        };

                        booksToAdd.Add(book);
                    }
                }
            }

            if (booksToAdd.Any())
            {
                // Ekleme işlemi (Ram'de bekler)
                await _unitOfWork.Books.AddRangeAsync(booksToAdd);
                // Tek seferde Kaydet (Transaction)
                await _unitOfWork.CommitAsync();
            }
        }

        public async Task<DataTableResponseDto<BookDto>> GetBooksForDataTableAsync(DataTableRequestDto request)
        {
            // 1. Sorguyu Başlat
            var query = _unitOfWork.Books.Where(x => true);

            // --- ADVANCED FILTERS (NEW) ---
            if (!string.IsNullOrEmpty(request.FilterCategory))
            {
                // Kategorinin içerip içermediğine bakalım (veya tam eşleşme)
                query = query.Where(x => x.AnaKategori != null && x.AnaKategori.Contains(request.FilterCategory));
            }
            if (!string.IsNullOrEmpty(request.FilterAuthor))
            {
                query = query.Where(x => x.Yazar != null && x.Yazar.Contains(request.FilterAuthor));
            }
            if (!string.IsNullOrEmpty(request.FilterYear))
            {
                query = query.Where(x => x.BasimYili != null && x.BasimYili.Contains(request.FilterYear));
            }

            // Numeric Filters
            if (request.FilterMinPrice.HasValue)
            {
                query = query.Where(x => x.Fiyat >= request.FilterMinPrice.Value);
            }
            if (request.FilterMaxPrice.HasValue)
            {
                query = query.Where(x => x.Fiyat <= request.FilterMaxPrice.Value);
            }
            
            if (request.FilterMinSales.HasValue)
            {
                query = query.Where(x => x.SatisRakamlari >= request.FilterMinSales.Value);
            }
            if (request.FilterMaxSales.HasValue)
            {
                query = query.Where(x => x.SatisRakamlari <= request.FilterMaxSales.Value);
            }

            // 2. Arama (Search) - NULL KONTROLLÜ GÜVENLİ VERSİYON
            if (request.Search != null && !string.IsNullOrEmpty(request.Search.Value))
            {
                var searchValue = request.Search.Value.ToLower();

                query = query.Where(x =>
                    (x.KitapAdi != null && x.KitapAdi.ToLower().Contains(searchValue)) ||
                    (x.Yazar != null && x.Yazar.ToLower().Contains(searchValue)) ||
                    (x.Yayinevi != null && x.Yayinevi.ToLower().Contains(searchValue)) ||
                    (x.AnaKategori != null && x.AnaKategori.ToLower().Contains(searchValue)) ||
                    (x.BasimYili != null && x.BasimYili.Contains(searchValue)) || // Yıl araması
                    (x.Fiyat.ToString().Contains(searchValue)) || // Fiyat araması
                    (x.SatisRakamlari.ToString().Contains(searchValue)) // Satış rakamı araması
                );
            }

            // 3. Toplam Kayıt Sayısını Al
            var totalRecordsFiltered = await query.CountAsync();

            // 4. Sıralama (Sorting) - NULL KONTROLLÜ
            if (request.Order != null && request.Order.Any() && request.Columns != null)
            {
                // Safe Access (?) kullanarak erişiyoruz
                var columnIndex = request.Order[0].Column;

                // Eğer index Columns listesinin boyutunu aşarsa hata vermesin
                if (columnIndex < request.Columns.Count)
                {
                    var orderColumn = request.Columns[columnIndex].Name;
                    var orderDir = request.Order[0].Dir;

                    // Sıralama Mantığı (Basit switch-case ile güvenli hale getirelim)
                    if (orderDir == "asc")
                    {
                        switch (orderColumn)
                        {
                            case "KitapAdi": query = query.OrderBy(x => x.KitapAdi); break;
                            case "Yazar": query = query.OrderBy(x => x.Yazar); break;
                            case "Fiyat": query = query.OrderBy(x => x.Fiyat); break;
                            case "SatisRakamlari": query = query.OrderBy(x => x.SatisRakamlari); break;
                            default: query = query.OrderBy(x => x.Id); break;
                        }
                    }
                    else
                    {
                        switch (orderColumn)
                        {
                            case "KitapAdi": query = query.OrderByDescending(x => x.KitapAdi); break;
                            case "Yazar": query = query.OrderByDescending(x => x.Yazar); break;
                            case "Fiyat": query = query.OrderByDescending(x => x.Fiyat); break;
                            case "SatisRakamlari": query = query.OrderByDescending(x => x.SatisRakamlari); break;
                            default: query = query.OrderByDescending(x => x.Id); break;
                        }
                    }
                }
            }
            else
            {
                // Varsayılan sıralama
                query = query.OrderByDescending(x => x.Id);
            }

            // 5. Sayfalama (Pagination)
            var data = await query
                .Skip(request.Start)
                .Take(request.Length)
                .ToListAsync();

            // 6. Entity -> DTO Dönüşümü
            var dataDto = _mapper.Map<List<BookDto>>(data);

            // 7. Cevap
            return new DataTableResponseDto<BookDto>
            {
                Draw = request.Draw,
                RecordsTotal = totalRecordsFiltered,
                RecordsFiltered = totalRecordsFiltered,
                Data = dataDto
            };
        }

        public async Task DeleteBookAsync(int id)
        {
            var book = await _unitOfWork.Books.Where(x => x.Id == id).FirstOrDefaultAsync();

            if (book != null)
            {
                _unitOfWork.Books.Remove(book);
                await _unitOfWork.CommitAsync();
            }
        }
        public async Task<BookDto> GetBookByIdAsync(int id)
        {
            var book = await _unitOfWork.Books.Where(b => b.Id == id).FirstOrDefaultAsync();
            if (book == null) throw new Exception("Kitap bulunamadı.");
            return _mapper.Map<BookDto>(book);
        }

        public async Task UpdateBookAsync(BookDto bookDto)
        {
            var book = await _unitOfWork.Books.Where(b => b.Id == bookDto.Id).FirstOrDefaultAsync();
            if (book != null)
            {
                book.KitapAdi = bookDto.KitapAdi;
                book.Yazar = bookDto.Yazar;
                book.Yayinevi = bookDto.Yayinevi;
                book.AnaKategori = bookDto.AnaKategori;
                book.Fiyat = bookDto.Fiyat ?? 0;
                book.SatisRakamlari = bookDto.SatisRakamlari;
                book.ISBN = bookDto.ISBN;
                book.SayfaSayisi = bookDto.SayfaSayisi;
                book.BasimYili = bookDto.BasimYili;
                
                _unitOfWork.Books.Update(book);
                await _unitOfWork.CommitAsync();
            }
        }

        public async Task ClearAllBooksAsync()
        {
            var allBooks = await _unitOfWork.Books.GetAllAsync();
            if (allBooks.Any())
            {
               foreach(var book in allBooks)
               {
                   _unitOfWork.Books.Remove(book);
               }
               await _unitOfWork.CommitAsync();
            }
        }

        public async Task AddBookAsync(BookCreateDto bookDto)
        {
            var book = new Book
            {
                KitapAdi = bookDto.KitapAdi,
                Yazar = bookDto.Yazar,
                Yayinevi = bookDto.Yayinevi,
                AnaKategori = bookDto.AnaKategori,
                Turu = bookDto.Turu,
                Fiyat = bookDto.Fiyat,
                SatisRakamlari = bookDto.SatisRakamlari,
                ISBN = bookDto.ISBN,
                SayfaSayisi = bookDto.SayfaSayisi,
                BasimYili = bookDto.BasimYili,
                KagitTipi = bookDto.KagitTipi,
                KapakTipi = bookDto.KapakTipi
                // Id otomatik artan
            };

            await _unitOfWork.Books.AddAsync(book);
            await _unitOfWork.CommitAsync();
        }
    }
}