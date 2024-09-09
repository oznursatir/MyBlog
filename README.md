# MyBlog   
# Blog Sitesi

Bu proje, C# dili ve .NET Core MVC çerçevesi kullanılarak oluşturulmuş bir blog sitesidir. Blog sitesi, kullanıcıların blog gönderileri oluşturmasına, düzenlemesine, silmesine ve yorum yapmasına olanak tanır.

## Özellikler

- Kullanıcı Kayıt ve Giriş İşlemleri
- Blog gönderileri oluşturma, düzenleme ve silme
- Kullanıcı rolleri (Admin, Editör, Kullanıcı)
- Yorum ekleme ve yorum düzenleme
- Blog gönderileri ve kullanıcılar üzerinde arama işlevi
- Sayfalama ve dinamik içerik yükleme (AJAX)
- Dosya yükleme ve CKEditor ile içerik oluşturma

## Kurulum

Projenin çalışabilmesi için aşağıdaki adımları izleyin:

1. **.NET Core SDK** kurulu olduğundan emin olun. [SDK'yi buradan indirin](https://dotnet.microsoft.com/download/dotnet).
2. Projeyi klonlayın:
   ```bash
   git clone https://github.com/oznursatir/MyBlog.git
3. Proje dizinine gidin:
   ```bash
   cd MyBlog  
4. Gerekli bağımlılıkları yükleyin:
   ```bash
   dotnet restore
5. Veritabanını migrate edin:

    1. **Migration Oluşturun:**
    
        ```bash
        Add-Migration InitialCreate

    2. **Veritabanını Güncelleyin:**

        ```bash
        Update-Database
        
6. Uygulamayı Çalıştırın:
       ```bash
       dotnet run  

### Kullanım
Uygulama, varsayılan olarak https://localhost:44371 adresinde çalışır. Tarayıcınızda bu adresi açarak blog sitesini kullanmaya başlayabilirsiniz.  

## Kullanıcı Paneli

Kullanıcı paneli, blog sitesinin standart kullanıcılarının kendi hesaplarını yönetmelerini ve site üzerinde etkileşimde bulunmalarını sağlar. Bu panel, kullanıcıların kişisel bilgilerini güncellemeleri, blog yazılarına yorum yapmaları ve kendi hesaplarıyla ilgili işlemleri gerçekleştirmeleri için gerekli araçları sunar.

### Özellikler

- **Profil Yönetimi:**
  - Kişisel bilgilerinizi güncelleyin, profil fotoğrafı ekleyin veya değiştirin.
  - Şifre değiştirme ve hesap yönetme.

- **Blog Yazılarına Yorum Yapma:**
  - Blog yazılarına yorum yapabilir, mevcut yorumları düzenleyebilir veya silebilirsiniz.  


### Kullanım

1. **Profil Yönetimi:**
   - **Adım 1:** Kullanıcı paneline giriş yapın ve "Profil" sekmesine gidin.
   - **Adım 2:** Kişisel bilgilerinizi güncellemek için gerekli alanları doldurun (isim, e-posta, vb.).
   - **Adım 3:** Profil fotoğrafınızı yükleyin veya değiştirin.
   - **Adım 4:** Şifrenizi değiştirmek için "Şifre Değiştir" bölümüne gidin ve yeni şifrenizi girin.
   - **Adım 5:** Değişiklikleri kaydedin.

2. **Blog Yazılarına Yorum Yapma:**
   - **Adım 1:** Okumak istediğiniz blog yazısını açın.
   - **Adım 2:** Yazının altına gidin ve yorum alanına yorumunuzu yazın.
   - **Adım 3:** "Yorum Yap" düğmesine tıklayarak yorumunuzu gönderin.
   - **Adım 4:** Mevcut yorumlarınızı düzenlemek veya silmek için yorumunuzu bulun ve ilgili düğmelere tıklayın.  


## Editör Paneli

Editör paneli, blog sitesinde içerik oluşturmak, yönetmek ve düzenlemek için kullanılır. Bu panel, her editör için kendi blog yazılarını oluşturma, düzenleme ve silme işlemleri gibi temel yönetim özelliklerini içerir.

### Özellikler

- **Yeni Blog Yazısı Oluşturma:** Editörler, zengin metin düzenleyici (CKEditor) kullanarak yeni blog yazıları oluşturabilir ve yayınlayabilir.
- **Mevcut Blog Yazılarını Düzenleme:** Editörler, var olan blog yazılarını düzenleyebilir, içerik güncelleyebilir, resim ekleyebilir veya değiştirebilir.
- **Blog Yazılarını Silme:** Editörler, istenmeyen veya hatalı blog yazılarını silebilir.  
- **Yorum Yönetimi:** Editörler yalnızca kendi yorumlarını, düzenleyebilir veya silebilir.

### Kullanım

1. **Yeni Blog Yazısı Oluşturma:**
   - **Adım 1:** Editör paneline giriş yapın ve "Yazı Oluştur" düğmesine tıklayın.
   - **Adım 2:** Başlık, içerik ve görsel alanlarını doldurun.
   - **Adım 3:** "Oluştur" düğmesine tıklayarak blog yazısını yayınlayın.

2. **Mevcut Blog Yazılarını Düzenleme:**
   - **Adım 1:** Editör panelinde "Bloglarım" sekmesine gidin.
   - **Adım 2:** Düzenlemek istediğiniz blog yazısının yanında bulunan "Düzenle" düğmesine tıklayın.
   - **Adım 3:** Gerekli değişiklikleri yapın ve "Gönder" düğmesine basın.

### Editör Paneline Erişim

Editör paneline erişim için kullanıcının "Editör" rolüne sahip olması gerekir. Bu rol, kullanıcı yönetim sayfasından yönetici tarafından atanabilir.

## Admin Paneli

Admin paneli, blog sitesinin tüm yönetimsel işlevlerine erişim sağlar. Bu panel, kullanıcı yönetimi, içerik yönetimi gibi özellikler sunar.

### Özellikler

- **Kullanıcı Yönetimi:**
  - Mevcut kullanıcıları düzenleyin veya silin.
  - Kullanıcı rollerini (Admin, Editör, Kullanıcı) yönetin.

- **İçerik Yönetimi:**
  - Blog yazılarını oluşturun.
  - Yayınlanmış içerikleri düzenleyin veya silin.

- **Yorum Yönetimi:**
  - Kullanıcı yorumlarını düzenleyin veya silin.

### Kullanım

1. **Kullanıcı Yönetimi:**
   - **Adım 1:** Admin panelinde "Yönetim" sekmesine gidin.
   - **Adım 2:** Kullanıcıları listeleyen tabloda mevcut kullanıcıları görüntüleyin.
   - **Adım 3:** Mevcut kullanıcıları düzenlemek veya silmek için ilgili kullanıcının yanındaki "Düzenle" veya "Sil" butonlarına tıklayın.  

2. **İçerik Yönetimi:**
   - **Adım 1:** Admin panelinde "Yönetim" sekmesine gidin.
   - **Adım 2:** İçerikleri listeleyen tabloda mevcut blog yazılarını görüntüleyin.
   - **Adım 7:** Var olan içerikleri düzenlemek veya silmek için ilgili blog yazısının yanındaki "Düzenle" veya "Sil" düğmesine tıklayın.  

3. **Yeni Blog Yazısı Oluşturma:**
   - **Adım 1:** Admin paneline giriş yapın ve "Yazı Oluştur" düğmesine tıklayın.
   - **Adım 2:** Başlık, içerik ve görsel alanlarını doldurun.
   - **Adım 3:** "Oluştur" düğmesine tıklayarak blog yazısını yayınlayın.
   - 
4. **Yorum Yönetimi:**
   - **Adım 1:** Yorumlar, blog gönderilerinin altında yer almaktadır.
   - **Adım 2:** İlgili blog yazısını açın ve yorumları görüntüleyin.
   - **Adım 3:** Yorumları düzenlemek veya silmek için ilgili yoruma tıklayın ve "Düzenle" veya "Sil" düğmesine tıklayın.

### Admin Paneline Erişim

Admin paneline erişim için kullanıcının "Admin" rolüne sahip olması gerekir. Bu rol, yalnızca süper kullanıcılar tarafından atanabilir ve admin paneline giriş yetkisi verir.




   
