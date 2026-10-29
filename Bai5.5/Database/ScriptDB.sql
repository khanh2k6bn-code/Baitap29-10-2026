CREATE DATABASE IF NOT EXISTS InventoryDB;
USE InventoryDB;

CREATE TABLE IF NOT EXISTS NhaCungCap (
    MaNCC INT AUTO_INCREMENT PRIMARY KEY,
    TenNCC VARCHAR(150) NOT NULL,
    DienThoai VARCHAR(15) UNIQUE,
    Diachi VARCHAR(255)
);

CREATE TABLE IF NOT EXISTS VatTu (
    MaVatTu VARCHAR(20) PRIMARY KEY,
    TenVatTu VARCHAR(150) NOT NULL,
    DVT VARCHAR(20),
    SoLuongTon DECIMAL(18,2) DEFAULT 0 CHECK (SoLuongTon >= 0)
);

CREATE TABLE IF NOT EXISTS PhieuKho (
    MaPhieu INT AUTO_INCREMENT PRIMARY KEY,
    LoaiPhieu VARCHAR(10) NOT NULL, -- 'NHAP' hoặc 'XUAT'
    NgayLap DATETIME DEFAULT CURRENT_TIMESTAMP,
    MaNCC INT NULL,
    TongTien DECIMAL(18,2) DEFAULT 0,
    FOREIGN KEY (MaNCC) REFERENCES NhaCungCap(MaNCC)
);

CREATE TABLE IF NOT EXISTS ChiTietPhieuKho (
    MaPhieu INT,
    MaVatTu VARCHAR(20),
    SoLuong DECIMAL(18,2) CHECK (SoLuong > 0),
    DonGia DECIMAL(18,2),
    PRIMARY KEY (MaPhieu, MaVatTu),
    FOREIGN KEY (MaPhieu) REFERENCES PhieuKho(MaPhieu),
    FOREIGN KEY (MaVatTu) REFERENCES VatTu(MaVatTu)
);

-- Dữ liệu mẫu
INSERT INTO NhaCungCap (TenNCC, DienThoai, Diachi) VALUES 
('Công ty Thép Miền Nam', '0901234567', 'Hà Nội'), 
('Tập đoàn Xi măng Vicem', '0918888999', 'Hải Phòng');

INSERT INTO VatTu (MaVatTu, TenVatTu, DVT, SoLuongTon) VALUES 
('VT01', 'Thép cuộn xây dựng', 'Kg', 500), 
('VT02', 'Xi măng PCB40', 'Bao', 300), 
('VT03', 'Gạch thẻ đặc', 'Viên', 1000);
