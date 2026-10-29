
CREATE DATABASE IF NOT EXISTS HotelDB;
USE HotelDB;

CREATE TABLE IF NOT EXISTS Phong (
    SoPhong VARCHAR(10) PRIMARY KEY,
    LoaiPhong VARCHAR(50),
    GiaTheoGio DECIMAL(18,2),
    TrangThai VARCHAR(30) DEFAULT 'Trống'
);

CREATE TABLE IF NOT EXISTS KhachHang (
    MaKH INT AUTO_INCREMENT PRIMARY KEY,
    HoTen VARCHAR(100) NOT NULL,
    CCCD VARCHAR(20) UNIQUE NOT NULL,
    SoDienThoai VARCHAR(15)
);

CREATE TABLE IF NOT EXISTS DichVu (
    MaDV INT AUTO_INCREMENT PRIMARY KEY,
    TenDV VARCHAR(100),
    DonGia DECIMAL(18,2)
);

CREATE TABLE IF NOT EXISTS PhieuDatPhong (
    MaPhieu INT AUTO_INCREMENT PRIMARY KEY,
    SoPhong VARCHAR(10),
    MaKH INT,
    NgayNhanPhong DATETIME,
    NgayTraPhong DATETIME NULL,
    TongTienThanhToan DECIMAL(18,2),
    FOREIGN KEY (SoPhong) REFERENCES Phong(SoPhong),
    FOREIGN KEY (MaKH) REFERENCES KhachHang(MaKH)
);

CREATE TABLE IF NOT EXISTS SuDungDichVu (
    MaPhieu INT,
    MaDV INT,
    SoLuong INT,
    ThoiGian DATETIME,
    PRIMARY KEY (MaPhieu, MaDV),
    FOREIGN KEY (MaPhieu) REFERENCES PhieuDatPhong(MaPhieu),
    FOREIGN KEY (MaDV) REFERENCES DichVu(MaDV)
);

-- Dữ liệu mẫu
INSERT INTO Phong (SoPhong, LoaiPhong, GiaTheoGio, TrangThai) VALUES 
('P101', 'Đơn', 80000, 'Trống'), 
('P102', 'Đơn', 80000, 'Trống'), 
('P201', 'Đôi', 120000, 'Trống'), 
('P202', 'Đôi', 120000, 'Trống'), 
('P301', 'VIP', 200000, 'Trống');

INSERT INTO DichVu (TenDV, DonGia) VALUES 
('Nước suối', 10000), 
('Mì tôm ly', 15000), 
('Giặt là ủi', 50000), 
('Bia hơi', 25000);
