const fs = require('fs');

const files = {
  showcase: 'C:/Users/pc/OneDrive/Masaüstü/FikirHavuzu/FikirHavuzu.Client/app/(main)/ideas/showcase/page.tsx',
  create: 'C:/Users/pc/OneDrive/Masaüstü/FikirHavuzu/FikirHavuzu.Client/app/(main)/ideas/create/page.tsx',
  my: 'C:/Users/pc/OneDrive/Masaüstü/FikirHavuzu/FikirHavuzu.Client/app/(main)/ideas/my/page.tsx',
  list: 'C:/Users/pc/OneDrive/Masaüstü/FikirHavuzu/FikirHavuzu.Client/app/(main)/ideas/list/page.tsx'
};

function injectTranslation(code) {
  if (!code.includes('useTranslation')) {
    code = code.replace(/import React(.*?);/, "import React$1;\nimport { useTranslation } from 'react-i18next';");
  }
  if (!code.includes('const { t } = useTranslation();')) {
    code = code.replace(/const \w+Page = \(\) => {/, "const $& \n    const { t } = useTranslation();");
  }
  return code;
}

// showcase
let showcaseCode = fs.readFileSync(files.showcase, 'utf8');
showcaseCode = injectTranslation(showcaseCode);
showcaseCode = showcaseCode.replace(/'📅 En Yeni Eklenenler'/g, "t('showcase.sortNewest')");
showcaseCode = showcaseCode.replace(/'⭐ En Yüksek Jüri Puanı'/g, "t('showcase.sortHighest')");
showcaseCode = showcaseCode.replace(/'🔤 Başlığa Göre \(A-Z\)'/g, "t('showcase.sortTitle')");
showcaseCode = showcaseCode.replace(/"Vitrin verileri yüklenirken hata:"/g, "t('showcase.fetchError')");
showcaseCode = showcaseCode.replace(/"🔒 Gizli Katılımcı"/g, "t('showcase.hiddenUser')");
showcaseCode = showcaseCode.replace(/>🌟 Pırıltılı Fikirler Vitrini</g, ">{t('showcase.title')}<");
showcaseCode = showcaseCode.replace(/>Kurum çalışanları tarafından önerilen ve süreçlerimize başarıyla uygulanan parlak inovasyon projeleri\.</g, ">{t('showcase.description')}<");
showcaseCode = showcaseCode.replace(/Toplam \{.*?\} Pırıltılı Fikir/g, "{t('showcase.totalIdeas', { count: allInnovators.reduce((sum, i) => sum + i.count, 0) })}");
showcaseCode = showcaseCode.replace(/>Yılın İnovatörleri Ödül Programı</g, ">{t('showcase.awardTitle')}<");
showcaseCode = showcaseCode.replace(/>\s*Her yıl sonunda podyumdaki ilk 3 personelimiz <strong>"Yılın İnovatörü"<\/strong> seçilir. Dereceye girenlere, o dönemin dinamiklerine ve çalışanın kendi tercihlerine göre esneklik gösterebilen sürpriz motivasyon ödülleri \(hediye çeki, ekstra izin vb\.\) sunulur\.\s*</g, "> <span dangerouslySetInnerHTML={{ __html: t('showcase.awardDescription') }} /> <");
showcaseCode = showcaseCode.replace(/>🏆 İnovasyon Liderlik Podyumu</g, ">{t('showcase.podiumTitle')}<");
showcaseCode = showcaseCode.replace(/tooltip="Puanlama & Sıralama Nasıl Hesaplanır\?"/g, "tooltip={t('showcase.podiumInfoTooltip')}");
showcaseCode = showcaseCode.replace(/>\(İnovasyon Puanı, Jüri Puanı & Yönetici Tercihi\)</g, ">{t('showcase.podiumSubTitle')}<");
showcaseCode = showcaseCode.replace(/"👑 İnovasyon Lideri"/g, "t('showcase.innovationLeader')");
showcaseCode = showcaseCode.replace(/"🌟 Pırıltılı İnovatör"/g, "t('showcase.glitteringInnovator')");
showcaseCode = showcaseCode.replace(/"✨ İnovatif Düşünür"/g, "t('showcase.innovativeThinker')");
showcaseCode = showcaseCode.replace(/>\(Siz\)</g, ">{t('showcase.you')}<");
showcaseCode = showcaseCode.replace(/\{innovator.points\} Puan/g, "{t('showcase.pointsBadge', { points: innovator.points })}");
showcaseCode = showcaseCode.replace(/\(Jüri: \{innovator.jurySum\}\)/g, "({t('showcase.juryScoreLabel')} {innovator.jurySum})");
showcaseCode = showcaseCode.replace(/label=\{`Tüm Sıralamayı Göster \(Toplam \$\{allInnovators.length\} Kişi\)`\}/g, "label={t('showcase.showAllRanking', { count: allInnovators.length })}");
showcaseCode = showcaseCode.replace(/placeholder="Proje başlığı veya çalışan ara\.\.\."/g, "placeholder={t('showcase.searchPlaceholder')}");
showcaseCode = showcaseCode.replace(/placeholder="Tüm Kategoriler"/g, "placeholder={t('showcase.allCategories')}");
showcaseCode = showcaseCode.replace(/tooltip="Filtreleri Temizle"/g, "tooltip={t('showcase.clearFilters')}");
showcaseCode = showcaseCode.replace(/>Aramanızla Eşleşen Pırıltılı Fikir Bulunamadı</g, ">{t('showcase.noMatchTitle')}<");
showcaseCode = showcaseCode.replace(/>\s*Vitrinin ışıkları kapalı çünkü henüz hayata geçen bir 'Pırıltılı Fikir' yok veya aramanızla eşleşen sonuç bulunamadı\.\s*</g, "> {t('showcase.noMatchDesc')} <");
showcaseCode = showcaseCode.replace(/label="Yeni Fikir Ekle"/g, "label={t('showcase.newIdeaBtn')}");
showcaseCode = showcaseCode.replace(/idea.categoryName \|\| "Diğer"/g, "idea.categoryName || t('showcase.otherCategory')");
showcaseCode = showcaseCode.replace(/> Pırıltılı Fikir \(\+150 Puan\)</g, "> {t('showcase.glitteringBadge')}<");
showcaseCode = showcaseCode.replace(/>Jüri Puanı: \{idea.latestScore\}\/100</g, ">{t('showcase.juryScoreBadge', { score: idea.latestScore })}<");
showcaseCode = showcaseCode.replace(/label="Detay"/g, "label={t('showcase.detailBtn')}");
showcaseCode = showcaseCode.replace(/>🎯 Sağlanan Kurumsal Fayda:</g, ">{t('showcase.benefitLabel')}<");
showcaseCode = showcaseCode.replace(/header="Puanlama Rehberi"/g, "header={t('showcase.guideTitle')}");
showcaseCode = showcaseCode.replace(/>İnovasyon Puanı Nedir\?</g, ">{t('showcase.guideInnovationTitle')}<");
showcaseCode = showcaseCode.replace(/>\s*• <strong>Olumlu Fikir:<\/strong> Hakemden 50\+ alan her onaylı fikir için <strong>\+50 Puan<\/strong>\.<br \/>\s*• <strong>Pırıltılı Fikir:<\/strong> Hayata geçip vitrine çıkan fikir için <strong>\+100 Ekstra Puan<\/strong> \*\(Fikir başına toplam 150 Puan\)\*\.\s*<\/p>/, "> <span dangerouslySetInnerHTML={{ __html: t('showcase.guideInnovationDesc1') }} /><br /> <span dangerouslySetInnerHTML={{ __html: t('showcase.guideInnovationDesc2') }} /> </p>");
showcaseCode = showcaseCode.replace(/>Jüri Puanı Nedir\?</g, ">{t('showcase.guideJuryTitle')}<");
showcaseCode = showcaseCode.replace(/>\s*Fikir Koordinatörlerinin fikirlere verdiği 0-100 arasındaki gerçek değerlendirme notlarının toplamıdır\.\s*<\/p>/, "> {t('showcase.guideJuryDesc')} </p>");
showcaseCode = showcaseCode.replace(/>3 Kademeli Sıralama & Eşitlik Bozma Kuralı:</g, ">{t('showcase.guideRuleTitle')}<");
showcaseCode = showcaseCode.replace(/<li className="mb-1"><strong>Toplam İnovasyon Puanı<\/strong> en yüksek olan çalışan öne geçer\.<\/li>/, "<li className=\"mb-1\" dangerouslySetInnerHTML={{ __html: t('showcase.guideRule1') }}></li>");
showcaseCode = showcaseCode.replace(/<li className="mb-1">Puanlar eşitse, <strong>Hakem Jüri Puanları Toplamı<\/strong> yüksek olan öne geçer\.<\/li>/, "<li className=\"mb-1\" dangerouslySetInnerHTML={{ __html: t('showcase.guideRule2') }}></li>");
showcaseCode = showcaseCode.replace(/<li>Jüri puanları da eşitse, <strong>Yönetici Tercihi<\/strong> ile lider belirlenir\.<\/li>/, "<li dangerouslySetInnerHTML={{ __html: t('showcase.guideRule3') }}></li>");
showcaseCode = showcaseCode.replace(/header="Genel İnovasyon Liderlik Sıralaması"/g, "header={t('showcase.leaderboardTitle')}");
showcaseCode = showcaseCode.replace(/header="Sıra"/g, "header={t('showcase.rankCol')}");
showcaseCode = showcaseCode.replace(/header="Çalışan"/g, "header={t('showcase.employeeCol')}");
showcaseCode = showcaseCode.replace(/header="Fikir Sayısı"/g, "header={t('showcase.ideaCountCol')}");
showcaseCode = showcaseCode.replace(/header="Jüri Notu"/g, "header={t('showcase.juryScoreCol')}");
showcaseCode = showcaseCode.replace(/header="İnovasyon Puanı"/g, "header={t('showcase.innovationScoreCol')}");
showcaseCode = showcaseCode.replace(/\{rowData.count\} Fikir/g, "{t('showcase.ideaCountBadge', { count: rowData.count })}");
showcaseCode = showcaseCode.replace(/\{rowData.points\} Puan/g, "{t('showcase.pointsBadge', { points: rowData.points })}");
fs.writeFileSync(files.showcase, showcaseCode, 'utf8');

// create
let createCode = fs.readFileSync(files.create, 'utf8');
createCode = injectTranslation(createCode);
createCode = createCode.replace(/"Kategoriler yüklenirken hata:"/g, "t('createIdea.fetchError')");
createCode = createCode.replace(/'Kategoriler yüklenemedi\.'/g, "t('createIdea.fetchErrorToast')");
createCode = createCode.replace(/'Eksik Bilgi'/g, "t('createIdea.missingInfo')");
createCode = createCode.replace(/'Lütfen yıldızlı \(\*\) tüm zorunlu alanları doldurun\.'/g, "t('createIdea.missingInfoDesc')");
createCode = createCode.replace(/'Başarılı'/g, "t('createIdea.success')");
createCode = createCode.replace(/'Hata'/g, "t('createIdea.error')");
createCode = createCode.replace(/'Fikir eklenirken sistemsel bir hata oluştu\.'/g, "t('createIdea.systemError')");
createCode = createCode.replace(/>✨ Yeni Fikir Önerisi</g, ">{t('createIdea.pageTitle')}<");
createCode = createCode.replace(/>Kurumumuzun geleceğine yön verecek yenilikçi fikrinizi bizimle paylaşın\.</g, ">{t('createIdea.pageDesc')}<");
createCode = createCode.replace(/>Fikir Başlığı \*</g, ">{t('createIdea.titleLabel')}<");
createCode = createCode.replace(/placeholder="Örn: Müşteri Geri Bildirim Süreçlerinin Yapay Zeka ile Otomasyonu"/g, "placeholder={t('createIdea.titlePlaceholder')}");
createCode = createCode.replace(/>Fikrinizi özetleyen net ve dikkat çekici bir başlık girin\.</g, ">{t('createIdea.titleHelp')}<");
createCode = createCode.replace(/>Kategori \*</g, ">{t('createIdea.categoryLabel')}<");
createCode = createCode.replace(/placeholder="-- Lütfen Bir Kategori Seçin --"/g, "placeholder={t('createIdea.categoryPlaceholder')}");
createCode = createCode.replace(/>Amaçlanan Kurumsal Fayda \/ Kazanım \*</g, ">{t('createIdea.benefitLabel')}<");
createCode = createCode.replace(/placeholder="Bu fikir hayata geçtiğinde ne gibi bir tasarruf, hız veya verimlilik sağlayacak\?"/g, "placeholder={t('createIdea.benefitPlaceholder')}");
createCode = createCode.replace(/>Detaylı Açıklama \*</g, ">{t('createIdea.descLabel')}<");
createCode = createCode.replace(/placeholder="Fikrinizin uygulanma adımlarını, yöntemini ve tüm detaylarını açıklayın\.\.\."/g, "placeholder={t('createIdea.descPlaceholder')}");
createCode = createCode.replace(/>Ek Dokümanlar \/ Belgeler \(İsteğe Bağlı\)</g, ">{t('createIdea.filesLabel')}<");
createCode = createCode.replace(/chooseLabel="Dosya Seç"/g, "chooseLabel={t('createIdea.fileSelect')}");
createCode = createCode.replace(/cancelLabel="İptal"/g, "cancelLabel={t('createIdea.fileCancel')}");
createCode = createCode.replace(/>Dosyaları buraya sürükleyip bırakabilirsiniz\.</g, ">{t('createIdea.fileEmpty')}<");
createCode = createCode.replace(/>Fikrinizi destekleyen PDF, Word, Excel, görsel vb\. dosyaları çoklu olarak seçip yükleyebilirsiniz\.</g, ">{t('createIdea.fileHelp')}<");
createCode = createCode.replace(/label="İptal"/g, "label={t('createIdea.btnCancel')}");
createCode = createCode.replace(/label="Taslak Olarak Kaydet"/g, "label={t('createIdea.btnDraft')}");
createCode = createCode.replace(/label="Fikri Havuza At"/g, "label={t('createIdea.btnSubmit')}");
fs.writeFileSync(files.create, createCode, 'utf8');

// my
let myCode = fs.readFileSync(files.my, 'utf8');
myCode = injectTranslation(myCode);
myCode = myCode.replace(/"Fikirler yüklenirken hata:"/g, "t('myIdeas.fetchErrorLog')");
myCode = myCode.replace(/'Fikirler yüklenemedi\.'/g, "t('myIdeas.fetchErrorToast')");
myCode = myCode.replace(/'Başarılı'/g, "t('myIdeas.success')");
myCode = myCode.replace(/'Hata'/g, "t('myIdeas.error')");
myCode = myCode.replace(/'Bir hata oluştu\.'/g, "t('myIdeas.genericError')");
myCode = myCode.replace(/'📅 En Yeni'/g, "t('myIdeas.sortNewest')");
myCode = myCode.replace(/'📅 En Eski'/g, "t('myIdeas.sortOldest')");
myCode = myCode.replace(/'🔤 Başlık \(A-Z\)'/g, "t('myIdeas.sortTitleAsc')");
myCode = myCode.replace(/'🔤 Başlık \(Z-A\)'/g, "t('myIdeas.sortTitleDesc')");
myCode = myCode.replace(/> Taslak</g, "> {t('myIdeas.statusDraft')}<");
myCode = myCode.replace(/> Pırıltılı Fikir</g, "> {t('myIdeas.statusGlittering')}<");
myCode = myCode.replace(/>Olumlu</g, ">{t('myIdeas.statusApproved')}<");
myCode = myCode.replace(/>Olumsuz</g, ">{t('myIdeas.statusRejected')}<");
myCode = myCode.replace(/>Geri Çekildi</g, ">{t('myIdeas.statusWithdrawn')}<");
myCode = myCode.replace(/>Bekleyen</g, ">{t('myIdeas.statusPending')}<");
myCode = myCode.replace(/label="Düzenle"/g, "label={t('myIdeas.btnEdit')}");
myCode = myCode.replace(/label="Havuza At"/g, "label={t('myIdeas.btnSubmit')}");
myCode = myCode.replace(/label="Geri Çek"/g, "label={t('myIdeas.btnWithdraw')}");
myCode = myCode.replace(/label="Detay"/g, "label={t('myIdeas.btnDetail')}");
myCode = myCode.replace(/label="Detayları Gör"/g, "label={t('myIdeas.btnViewDetail')}");
myCode = myCode.replace(/>✨ Fikirlerim</g, ">{t('myIdeas.pageTitle')}<");
myCode = myCode.replace(/>Sisteme sunduğunuz veya taslak halindeki tüm fikirlerinizi buradan takip edebilirsiniz\.</g, ">{t('myIdeas.pageDesc')}<");
myCode = myCode.replace(/label="Tüm"/g, "label={t('myIdeas.filterAll')}");
myCode = myCode.replace(/label="📝 Taslak"/g, "label={t('myIdeas.filterDraft')}");
myCode = myCode.replace(/label="⏳ Bekleyen"/g, "label={t('myIdeas.filterPending')}");
myCode = myCode.replace(/label="✅ Olumlu"/g, "label={t('myIdeas.filterApproved')}");
myCode = myCode.replace(/label="❌ Olumsuz"/g, "label={t('myIdeas.filterRejected')}");
myCode = myCode.replace(/label="Yeni Fikir Ekle"/g, "label={t('myIdeas.newIdeaBtn')}");
myCode = myCode.replace(/emptyMessage="Bu filtre kriterine uygun bir fikir öneriniz bulunamadı\."/g, "emptyMessage={t('myIdeas.emptyMessage')}");
myCode = myCode.replace(/header="Başlık"/g, "header={t('myIdeas.colTitle')}");
myCode = myCode.replace(/header="Kategori"/g, "header={t('myIdeas.colCategory')}");
myCode = myCode.replace(/header="Oluşturma Tarihi"/g, "header={t('myIdeas.colDate')}");
myCode = myCode.replace(/header="Durum"/g, "header={t('myIdeas.colStatus')}");
myCode = myCode.replace(/header="İşlemler"/g, "header={t('myIdeas.colActions')}");
myCode = myCode.replace(/rowData.categoryName \|\| 'Diğer'/g, "rowData.categoryName || t('myIdeas.otherCategory')");
myCode = myCode.replace(/>Bu ışıltılı hayatı o seçmedi\.\.\.</g, ">{t('myIdeas.withdrawTitle')}<");
myCode = myCode.replace(/"<strong className="text-900">/g, "{t('myIdeas.withdrawConfirmText1')}<strong className=\"text-900\">");
myCode = myCode.replace(/<\/strong>" fikrini geri çekmek istediğine gerçekten emin misin\?/g, "</strong>{t('myIdeas.withdrawConfirmText2')}");
myCode = myCode.replace(/> Aramızdan ayrılan sadece bir taslak, asıl ışıltın hâlâ sende saklı\. <br \/>/g, "> {t('myIdeas.withdrawInfoTitle')} <br />");
myCode = myCode.replace(/><em>\(Fikriniz değerlendirmeden çekilse dahi sistem arşivinde denetim amacıyla saklanmaya devam eder\)\.<\/em></g, "><em>{t('myIdeas.withdrawInfoDesc')}</em><");
myCode = myCode.replace(/label="Hala ışığı var\. ✨"/g, "label={t('myIdeas.btnCancelWithdraw')}");
myCode = myCode.replace(/label="Veda vakti geldi\. 🪦"/g, "label={t('myIdeas.btnConfirmWithdraw')}");
fs.writeFileSync(files.my, myCode, 'utf8');

// list
let listCode = fs.readFileSync(files.list, 'utf8');
listCode = injectTranslation(listCode);
listCode = listCode.replace(/"Fikirler yüklenirken hata:"/g, "t('ideasList.fetchErrorLog')");
listCode = listCode.replace(/'Yetkisiz Erişim'/g, "t('ideasList.unauthorizedToastTitle')");
listCode = listCode.replace(/'Bu sayfayı görüntüleme yetkiniz yok\.'/g, "t('ideasList.unauthorizedToastDesc')");
listCode = listCode.replace(/'📅 En Yeni'/g, "t('ideasList.sortNewest')");
listCode = listCode.replace(/'📅 En Eski'/g, "t('ideasList.sortOldest')");
listCode = listCode.replace(/'⭐ En Yüksek Puan'/g, "t('ideasList.sortScoreDesc')");
listCode = listCode.replace(/'⭐ En Düşük Puan'/g, "t('ideasList.sortScoreAsc')");
listCode = listCode.replace(/'🔤 Başlık \(A-Z\)'/g, "t('ideasList.sortTitleAsc')");
listCode = listCode.replace(/'🔤 Başlık \(Z-A\)'/g, "t('ideasList.sortTitleDesc')");
listCode = listCode.replace(/"🔒 Gizli Katılımcı"/g, "t('ideasList.hiddenUser')");
listCode = listCode.replace(/> Anonim \(Kör Değerlendirme\)</g, "> {t('ideasList.anonymous')}<");
listCode = listCode.replace(/>\(Siz\)</g, ">{t('ideasList.you')}<");
listCode = listCode.replace(/> Pırıltılı Fikir</g, "> {t('ideasList.statusGlittering')}<");
listCode = listCode.replace(/>Olumlu</g, ">{t('ideasList.statusApproved')}<");
listCode = listCode.replace(/>Olumsuz</g, ">{t('ideasList.statusRejected')}<");
listCode = listCode.replace(/>Bekleyen</g, ">{t('ideasList.statusPending')}<");
listCode = listCode.replace(/label="İncele"/g, "label={t('ideasList.btnReview')}");
listCode = listCode.replace(/label="İncele \/ Puanla"/g, "label={t('ideasList.btnReviewRate')}");
listCode = listCode.replace(/>✨ Fikir ve Öneriler</g, ">{t('ideasList.pageTitle')}<");
listCode = listCode.replace(/>Kurum çalışanları tarafından sunulan tüm inovasyon önerilerini inceleyin ve puanlayın\.</g, ">{t('ideasList.pageDesc')}<");
listCode = listCode.replace(/label="Tüm"/g, "label={t('ideasList.filterAll')}");
listCode = listCode.replace(/label="⏳ Bekleyen"/g, "label={t('ideasList.filterPending')}");
listCode = listCode.replace(/label="✅ Olumlu"/g, "label={t('ideasList.filterApproved')}");
listCode = listCode.replace(/label="❌ Olumsuz"/g, "label={t('ideasList.filterRejected')}");
listCode = listCode.replace(/emptyMessage="Bu filtre kriterine uygun bir fikir önerisi bulunamadı\."/g, "emptyMessage={t('ideasList.emptyMessage')}");
listCode = listCode.replace(/header="Fikir Sahibi"/g, "header={t('ideasList.colAuthor')}");
listCode = listCode.replace(/header="Fikir Başlığı"/g, "header={t('ideasList.colTitle')}");
listCode = listCode.replace(/header="Kategori"/g, "header={t('ideasList.colCategory')}");
listCode = listCode.replace(/header="Tarih"/g, "header={t('ideasList.colDate')}");
listCode = listCode.replace(/header="Durum"/g, "header={t('ideasList.colStatus')}");
listCode = listCode.replace(/header="İşlemler"/g, "header={t('ideasList.colActions')}");
listCode = listCode.replace(/rowData.categoryName \|\| 'Diğer'/g, "rowData.categoryName || t('ideasList.otherCategory')");
fs.writeFileSync(files.list, listCode, 'utf8');

console.log('TSX files updated.');
