using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarCAD.Entity.Concrete;
using StarCAD.Common.Constants;

namespace StarCAD.DataAccess.Concrete.EntityFramework.Configurations;

public class CodeConfiguration : IEntityTypeConfiguration<Code>
{
    public void Configure(EntityTypeBuilder<Code> builder)
    {
        builder.ToTable("Codes");

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.CodeValue).HasMaxLength(50).IsRequired();


        builder.HasIndex(x => new { x.GroupId, x.CodeValue }).IsUnique();

        builder.HasData(
            
            // =========================
            // Mimari Araçlar (GroupId = 1)
            // =========================
            new Code { Id = 1001, GroupId = 1, CodeValue = "Switch", Name = "Ana Kesme Şalteri Ekle", IsActive = true },
            new Code { Id = 1002, GroupId = 1, CodeValue = "Wall", Name = "Duvar Çiz", IsActive = true },
            new Code { Id = 1003, GroupId = 1, CodeValue = "Panel", Name = "Pano Ekle", IsActive = true },
            new Code { Id = 1004, GroupId = 1, CodeValue = "Room", Name = "Oda Çiz", IsActive = true },
            new Code { Id = 1005, GroupId = 1, CodeValue = "Lighting", Name = "Aydınlatma Ekle", IsActive = true },
            new Code { Id = 1006, GroupId = 1, CodeValue = "Door", Name = "Kapı Ekle", IsActive = true },
            new Code
            {
                Id = 1007, GroupId = 1, CodeValue = "FireExtinguisher", Name = "Yangın Söndürücü Ekle", IsActive = true
            },
            new Code { Id = 1008, GroupId = 1, CodeValue = "Window", Name = "Pencere Ekle", IsActive = true },
            new Code { Id = 1009, GroupId = 1, CodeValue = "Eraser", Name = "Silgi", IsActive = true },
            new Code { Id = 1010, GroupId = 1, CodeValue = "Staircase", Name = "Merdiven Ekle", IsActive = true },
            new Code { Id = 1011, GroupId = 1, CodeValue = "BulkDelete", Name = "Toplu Silme", IsActive = true },
            new Code { Id = 1012, GroupId = 1, CodeValue = "Column", Name = "Kolon Ekle", IsActive = true },
            new Code { Id = 1013, GroupId = 1, CodeValue = "Text", Name = "Metin Ekle", IsActive = true },
            new Code { Id = 1014, GroupId = 1, CodeValue = "Beam", Name = "Kiriş Ekle", IsActive = true },
            new Code { Id = 1015, GroupId = 1, CodeValue = "Measure", Name = "Ölçüm", IsActive = true },
            new Code { Id = 1016, GroupId = 1, CodeValue = "Vent", Name = "Menfez Ekle", IsActive = true },
            new Code
            {
                Id = 1017, GroupId = 1, CodeValue = "FreeDraw", Name = "Serbest Çizim Araçları", IsActive = true
            },
            new Code
            {
                Id = 1018, GroupId = 1, CodeValue = "RoofVent", Name = "Kolon Havalandırması Ekle", IsActive = true
            },
            new Code { Id = 1019, GroupId = 1, CodeValue = "Alarm", Name = "Alarm Cihazı Ekle", IsActive = true },
            new Code
            {
                Id = 1020, GroupId = 1, CodeValue = "EarthquakeSensor", Name = "Deprem Sensörü Ekle", IsActive = true
            },
            new Code { Id = 1021, GroupId = 1, CodeValue = "FlueShaft", Name = "Baca Şaftı Ekle", IsActive = true },

            // =========================
            // Proje Durumları (GroupId = 2)
            // =========================
            new Code { Id = 2001, GroupId = 2, CodeValue = CodeValues.Draft, Name = "Taslak", IsActive = true },
            new Code
            {
                Id = 2003, GroupId = 2, CodeValue = CodeValues.PendingApproval, Name = "Onay Bekleyen", IsActive = true
            },
            new Code { Id = 2002, GroupId = 2, CodeValue = CodeValues.Approved, Name = "Onaylanan", IsActive = true },
            new Code { Id = 2004, GroupId = 2, CodeValue = CodeValues.Rejected, Name = "Reddedilen", IsActive = true },

            // =========================
            // Tesisat Araçları (GroupId = 3)
            // =========================
            new Code { Id = 3001, GroupId = 3, CodeValue = "Regulator", Name = "Regülatör Ekle", IsActive = true },
            new Code { Id = 3002, GroupId = 3, CodeValue = "Pipe", Name = "Boru Ekle", IsActive = true },
            new Code { Id = 3003, GroupId = 3, CodeValue = "FluePoint", Name = "Baca Ekle", IsActive = true },
            new Code { Id = 3004, GroupId = 3, CodeValue = "BranchPipe", Name = "Branşman Ekle", IsActive = true },
            new Code { Id = 3005, GroupId = 3, CodeValue = "Isolation", Name = "İzolasyon Ekle", IsActive = true },
            new Code { Id = 3006, GroupId = 3, CodeValue = "GasMeter", Name = "Sayaç Ekle", IsActive = true },
            new Code { Id = 3007, GroupId = 3, CodeValue = "Manometer", Name = "Manometre Ekle", IsActive = true },
            new Code { Id = 3008, GroupId = 3, CodeValue = "ServiceBox", Name = "Servis Kutusu Ekle", IsActive = true },
            new Code { Id = 3009, GroupId = 3, CodeValue = "Filter", Name = "Filtre / Kit Ekle", IsActive = true },
            new Code { Id = 3010, GroupId = 3, CodeValue = "Valve", Name = "Vana Ekle", IsActive = true },
            new Code { Id = 3011, GroupId = 3, CodeValue = "DumbGasMeter", Name = "Süzme Sayaç Ekle", IsActive = true },
            new Code
            {
                Id = 3012, GroupId = 3, CodeValue = "SolenoidValve", Name = "Selenoid Vana Ekle", IsActive = true
            },
            new Code
            {
                Id = 3013, GroupId = 3, CodeValue = "VentilationPoint", Name = "Havalandırma Kanalı Ekle",
                IsActive = true
            },
            new Code { Id = 3014, GroupId = 3, CodeValue = "Cooker", Name = "Ocak Ekle", IsActive = true },
            new Code { Id = 3015, GroupId = 3, CodeValue = "Measure", Name = "Ölçüm", IsActive = true },

            // =========================
            // Evrak Tipleri (GroupId = 5)
            // =========================
            new Code { Id = 5001, GroupId = 5, CodeValue = "BacaReport", Name = "Baca Raporu", IsActive = true },
            new Code
            {
                Id = 5002, GroupId = 5, CodeValue = "DeviceFlueExhaustCertificate", Name = "Cihaz Baca Atış Belgesi",
                IsActive = true
            },
            new Code
            {
                Id = 5003, GroupId = 5, CodeValue = "MinimumConsumptionDeclaration",
                Name = "Cihaz Minimum Tüketim Beyanı", IsActive = true
            },
            new Code
            {
                Id = 5004, GroupId = 5, CodeValue = "DeviceServiceReport", Name = "Cihaz Servis Kontrol Raporu",
                IsActive = true
            },
            new Code
            {
                Id = 5005, GroupId = 5, CodeValue = "DeviceStandardCertificate", Name = "Cihaz Standart Belgesi",
                IsActive = true
            },
            new Code
            {
                Id = 5006, GroupId = 5, CodeValue = "DeviceComplianceCertificate", Name = "Cihaz Uygunluk Belgesi",
                IsActive = true
            },
            new Code { Id = 5007, GroupId = 5, CodeValue = "DaskPolicy", Name = "DASK Poliçesi", IsActive = true },
            new Code
            {
                Id = 5008, GroupId = 5, CodeValue = "NaturalGasCompliance", Name = "Doğalgaz Uygunluk Belgesi",
                IsActive = true
            },
            new Code
            {
                Id = 5009, GroupId = 5, CodeValue = "FlexibleInstallationTraining",
                Name = "Esnek Tesisat Eğitim Belgesi", IsActive = true
            },
            new Code
            {
                Id = 5010, GroupId = 5, CodeValue = "FlexibleInstallationMYK", Name = "Esnek Tesisat MYK Belgesi",
                IsActive = true
            },
            new Code { Id = 5011, GroupId = 5, CodeValue = "FavoriteDocument", Name = "Favori Evrak", IsActive = true },
            new Code
            {
                Id = 5012, GroupId = 5, CodeValue = "GasCompetencyCertificate", Name = "Gaz Yeterlilik Belgesi",
                IsActive = true
            },
            new Code { Id = 5013, GroupId = 5, CodeValue = "GeneralDocument", Name = "Genel Evrak", IsActive = true },
            new Code
            {
                Id = 5014, GroupId = 5, CodeValue = "LocationComplianceCertificate", Name = "Mahal Uygunluk Belgesi",
                IsActive = true
            },
            new Code
            {
                Id = 5015, GroupId = 5, CodeValue = "CustomerAgreement", Name = "Müşteri Sözleşmesi", IsActive = true
            },
            new Code { Id = 5016, GroupId = 5, CodeValue = "AddressNumbering", Name = "Numarataj", IsActive = true },
            new Code { Id = 5019, GroupId = 5, CodeValue = "License", Name = "Ruhsat", IsActive = true },

            // =========================
            // Proje Tipi (GroupId = 7)
            // =========================
            new Code { Id = 7001, GroupId = 7, CodeValue = "ILAVE", Name = "İlave", IsActive = true },
            new Code { Id = 7002, GroupId = 7, CodeValue = "ILAVE_TADILAT", Name = "İlave Tadilat", IsActive = true },
            new Code { Id = 7003, GroupId = 7, CodeValue = "KOLON", Name = "Kolon", IsActive = true },
            new Code { Id = 7004, GroupId = 7, CodeValue = "KOLON_TADILAT", Name = "Kolon Tadilat", IsActive = true },

            // =========================
            // Isınma Tipi (GroupId = 8)
            // =========================
            new Code { Id = 8001, GroupId = 8, CodeValue = "bireysel", Name = "Bireysel", IsActive = true },
            new Code { Id = 8002, GroupId = 8, CodeValue = "merkezi", Name = "Merkezi", IsActive = true },

            // =========================
            // Bina Kullanımı Tipi (GroupId = 9)
            // =========================
            new Code { Id = 9001, GroupId = 9, CodeValue = "coklu", Name = "Çoklu", IsActive = true },
            new Code { Id = 9002, GroupId = 9, CodeValue = "mustakil", Name = "Müstakil", IsActive = true },

            // =========================
            // Servis Kutusu Modeli (GroupId = 32)
            // =========================
            new Code { Id = 3201, GroupId = 32, CodeValue = "CES200", Name = "CES 200", IsActive = true },
            new Code { Id = 3202, GroupId = 32, CodeValue = "S200", Name = "S 200", IsActive = true },
            new Code { Id = 3203, GroupId = 32, CodeValue = "S300", Name = "S 300", IsActive = true });
    }
}