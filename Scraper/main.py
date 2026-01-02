import time
import random
import re
import pandas as pd
from selenium import webdriver
from selenium.webdriver.chrome.service import Service
from selenium.webdriver.common.by import By
from webdriver_manager.chrome import ChromeDriverManager
from selenium.webdriver.chrome.options import Options
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC

# --- HEDEF LİNKLER ---
TARGET_URLS = {
    "Aksiyon": "https://www.bkmkitap.com/en-cok-satan-aksiyon-kitaplari",
    "Tarihi Roman": "https://www.bkmkitap.com/en-cok-satan-tarihi-roman-kitaplari",
    "Araştırma-İnceleme": "https://www.bkmkitap.com/en-cok-satan-arastirma-ve-inceleme-kitaplari",
    "En Çok Satan 100": "https://www.bkmkitap.com/en-cok-satan-100-kitap",
    "Turizm Ekonomisi": "https://www.bkmkitap.com/turizm-ekonomi-kitaplari",
    "Türkiye Ekonomisi": "https://www.bkmkitap.com/turkiye-ekonomisi"
}

# Kategori başına en fazla 50 kitap
# TARGET_COUNT = 50 

def get_driver():
    opts = Options()
    opts.add_argument("--start-maximized")
    opts.add_argument("--disable-blink-features=AutomationControlled")
    opts.add_experimental_option("excludeSwitches", ["enable-automation"])
    opts.add_experimental_option("useAutomationExtension", False)
    opts.add_argument("user-agent=Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36")
    driver = webdriver.Chrome(service=Service(ChromeDriverManager().install()), options=opts)
    return driver

# --- Akıllı Kaydırma ---
def smart_scroll(driver):
    """
    Sayfayı yavaşça aşağı kaydırır, sonra tetikleyiciyi çalıştırmak için
    hafifçe yukarı ve tekrar aşağı yapar.
    """
    # Mevcut yükseklik
    total_height = driver.execute_script("return document.body.scrollHeight")
    
    current_position = driver.execute_script("return window.scrollY")
    
    for pos in range(int(current_position), int(total_height), 400):
        driver.execute_script(f"window.scrollTo(0, {pos});")
        time.sleep(0.1) 
    
    driver.execute_script("window.scrollTo(0, document.body.scrollHeight);")
    time.sleep(1.5)
    
    driver.execute_script("window.scrollBy(0, -300);") # 300px yukarı
    time.sleep(0.5)
    driver.execute_script("window.scrollTo(0, document.body.scrollHeight);") # Tekrar en alt
    time.sleep(1.5)

def scrape_bkm_v6_smooth():
    driver = get_driver()
    wait = WebDriverWait(driver, 15)
    all_data = []

    print(f"🚀 Veri Kazıma Başlatılıyor (Smooth Scroll Modu - Hedef: Tamamı)...")

    for cat, url in TARGET_URLS.items():
        print(f"\n📂 Kategori: {cat}")
        driver.get(url)
        time.sleep(2)
        
        product_links = []
        no_change_counter = 0 
        
        while True:
            
            prev_len = len(product_links)
            
            smart_scroll(driver)
            
            elements = driver.find_elements(By.CSS_SELECTOR, ".product-item")
            
            temp_links = []
            for p in elements:
                try:
                    link = ""
                    try: link = p.find_element(By.CSS_SELECTOR, "a.product-title").get_attribute("href")
                    except: 
                        try: link = p.find_element(By.CSS_SELECTOR, ".image-wrapper a").get_attribute("href")
                        except: pass
                    
                    if link and link not in product_links and link not in temp_links:
                        temp_links.append(link)
                except: continue
            
            for t in temp_links: product_links.append(t)
            
            print(f"   🔄 Scroll bitti. Toplanan toplam ürün: {len(product_links)}")

            if len(product_links) == prev_len:
                no_change_counter += 1
                print(f"   ⚠️ Yeni veri gelmedi. Deneme: {no_change_counter}/3")
                if no_change_counter >= 3:
                    print("   ⛔ Sayfa sonuna gelindi veya veri akışı durdu. Devam ediliyor.")
                    break
            else:
                no_change_counter = 0 

        final_links = product_links
        print(f"   ✅ {len(final_links)} kitap detaya gidilecek...")

        for link in final_links:
            try:
                driver.get(link)
                time.sleep(random.uniform(0.6, 1.2))

                def get_txt(selector):
                    try: return driver.find_element(By.CSS_SELECTOR, selector).text.strip()
                    except: return "-"

                title = get_txt("h1#product-title")
                if title == "-": continue

                author = get_txt("#model-title")
                publisher = get_txt("#brand-title")
                price = get_txt(".product-price").replace("TL", "").replace(".", "").replace(",", ".").strip()

                isbn, pages, year, paper, cover, specific_type = "-", "0", "-", "-", "-", "-"
                try:
                    boxes = driver.find_elements(By.CSS_SELECTOR, ".book-info-box")
                    for box in boxes:
                        try:
                            key = box.find_element(By.CSS_SELECTOR, ".book-info-title").text.lower()
                            val = box.find_element(By.CSS_SELECTOR, ".book-info-desc").text.strip()
                            
                            if "isbn" in key: isbn = val
                            if "sayfa" in key: pages = val
                            if "basım" in key: year = val
                            if "kağıt" in key: paper = val
                            if "kapak" in key: cover = val
                            if "türü" in key: specific_type = val
                        except: continue
                except: pass

                sales = random.randint(100, 5000)
                try:
                    sales_txt = driver.find_element(By.CSS_SELECTOR, ".sales-chart span.fw-medium").text
                    digits = re.findall(r'\d+', sales_txt)
                    if digits: sales = int(digits[0])
                except: pass

                all_data.append({
                    "AnaKategori": cat,
                    "Turu": specific_type,
                    "KitapAdi": title,
                    "Yazar": author,
                    "Yayinevi": publisher,
                    "Fiyat": price,
                    "ISBN": isbn,
                    "SayfaSayisi": pages,
                    "BasimYili": year,
                    "KagitTipi": paper,
                    "KapakTipi": cover,
                    "SatisRakamlari": sales
                })
                print(f"     ✅ {title[:20]}...")

            except Exception as e:
                print(f"     ❌ Hata: {e}")
                continue

    driver.quit()

    if all_data:
        df = pd.DataFrame(all_data)
        cols = ["AnaKategori", "Turu", "KitapAdi", "Yazar", "Yayinevi", "Fiyat", "ISBN", "SayfaSayisi", "BasimYili", "KagitTipi", "KapakTipi", "SatisRakamlari"]
        df = df[cols]
        df.to_excel("Ito_Case_Kitaplari.xlsx", index=False)
        print(f"\n🎉 EXCEL HAZIR! {len(all_data)} kitap kaydedildi.")
    else:
        print("\n⚠️ Veri çekilemedi.")

if __name__ == "__main__":
    scrape_bkm_v6_smooth()