CREATE DATABASE IF NOT EXISTS CafePOSDB;
USE CafePOSDB;

CREATE TABLE IF NOT EXISTS BanAn (
    MaBan INT AUTO_INCREMENT PRIMARY KEY,
    TenBan VARCHAR(50) NOT NULL,
    TrangThai VARCHAR(20) DEFAULT 'Trống'
);

CREATE TABLE IF NOT EXISTS DanhMuc (
    MaDanhMuc INT AUTO_INCREMENT PRIMARY KEY,
    TenDanhMuc VARCHAR(100) NOT NULL
);

CREATE TABLE IF NOT EXISTS MonAn (
    MaMon INT AUTO_INCREMENT PRIMARY KEY,
    TenMon VARCHAR(100) NOT NULL,
    MaDanhMuc INT,
    DonGia DECIMAL(18,2),
    FOREIGN KEY (MaDanhMuc) REFERENCES DanhMuc(MaDanhMuc)
);

CREATE TABLE IF NOT EXISTS HoaDon (
    MaHoaDon INT AUTO_INCREMENT PRIMARY KEY,
    MaBan INT,
    GioVao DATETIME,
    GioRa DATETIME,
    TongTien DECIMAL(18,2),
    TrangThai INT DEFAULT 0, 
    FOREIGN KEY (MaBan) REFERENCES BanAn(MaBan)
);

CREATE TABLE IF NOT EXISTS ChiTietHoaDon (
    MaHoaDon INT,
    MaMon INT,
    SoLuong INT,
    DonGia DECIMAL(18,2),
    PRIMARY KEY (MaHoaDon, MaMon),
    FOREIGN KEY (MaHoaDon) REFERENCES HoaDon(MaHoaDon),
    FOREIGN KEY (MaMon) REFERENCES MonAn(MaMon)
);

-- Dữ liệu mẫu
INSERT INTO BanAn (TenBan, TrangThai) VALUES ('Bàn 01', 'Trống'), ('Bàn 02', 'Trống'), ('Bàn 03', 'Trống'), ('Bàn 04', 'Trống'), ('Bàn 05', 'Trống'), ('Bàn 06', 'Trống');
INSERT INTO DanhMuc (TenDanhMuc) VALUES ('Cà phê'), ('Trà sữa'), ('Đồ ăn nhẹ');
INSERT INTO MonAn (TenMon, MaDanhMuc, DonGia) VALUES ('Cà phê đen', 1, 25000), ('Cà phê sữa', 1, 30000), ('Trà sữa trân châu', 2, 45000), ('Trà sữa thái xanh', 2, 40000), ('Hướng dương', 3, 15000), ('Khoai tây chiên', 3, 35000);
