/*
 Navicat MySQL Data Transfer

 Source Server         : Localhost
 Source Server Type    : MySQL
 Source Server Version : 101116
 Source Host           : localhost:3306
 Source Schema         : inventorydb

 Target Server Type    : MySQL
 Target Server Version : 101116
 File Encoding         : 65001

 Date: 29/10/2026 15:35:21
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for chitietphieukho
-- ----------------------------
DROP TABLE IF EXISTS `chitietphieukho`;
CREATE TABLE `chitietphieukho`  (
  `MaPhieu` int NOT NULL,
  `MaVatTu` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `SoLuong` decimal(18, 2) NULL DEFAULT NULL,
  `DonGia` decimal(18, 2) NULL DEFAULT NULL,
  PRIMARY KEY (`MaPhieu`, `MaVatTu`) USING BTREE,
  INDEX `MaVatTu`(`MaVatTu`) USING BTREE,
  CONSTRAINT `chitietphieukho_ibfk_1` FOREIGN KEY (`MaPhieu`) REFERENCES `phieukho` (`MaPhieu`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `chitietphieukho_ibfk_2` FOREIGN KEY (`MaVatTu`) REFERENCES `vattu` (`MaVatTu`) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of chitietphieukho
-- ----------------------------
INSERT INTO `chitietphieukho` VALUES (1, 'VT01', 10000.00, 50000000000.00);

-- ----------------------------
-- Table structure for nhacungcap
-- ----------------------------
DROP TABLE IF EXISTS `nhacungcap`;
CREATE TABLE `nhacungcap`  (
  `MaNCC` int NOT NULL AUTO_INCREMENT,
  `TenNCC` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `DienThoai` varchar(15) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `Diachi` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  PRIMARY KEY (`MaNCC`) USING BTREE,
  UNIQUE INDEX `DienThoai`(`DienThoai`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 3 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of nhacungcap
-- ----------------------------
INSERT INTO `nhacungcap` VALUES (1, 'Công ty Thép Miền Nam', '0901234567', 'Hà Nội');
INSERT INTO `nhacungcap` VALUES (2, 'Tập đoàn Xi măng Vicem', '0918888999', 'Hải Phòng');

-- ----------------------------
-- Table structure for phieukho
-- ----------------------------
DROP TABLE IF EXISTS `phieukho`;
CREATE TABLE `phieukho`  (
  `MaPhieu` int NOT NULL AUTO_INCREMENT,
  `LoaiPhieu` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `NgayLap` datetime NULL DEFAULT current_timestamp,
  `MaNCC` int NULL DEFAULT NULL,
  `TongTien` decimal(18, 2) NULL DEFAULT 0.00,
  PRIMARY KEY (`MaPhieu`) USING BTREE,
  INDEX `MaNCC`(`MaNCC`) USING BTREE,
  CONSTRAINT `phieukho_ibfk_1` FOREIGN KEY (`MaNCC`) REFERENCES `nhacungcap` (`MaNCC`) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 2 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of phieukho
-- ----------------------------
INSERT INTO `phieukho` VALUES (1, 'NHAP', '2026-10-29 15:22:56', 1, 500000000000000.00);

-- ----------------------------
-- Table structure for vattu
-- ----------------------------
DROP TABLE IF EXISTS `vattu`;
CREATE TABLE `vattu`  (
  `MaVatTu` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `TenVatTu` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `DVT` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `SoLuongTon` decimal(18, 2) NULL DEFAULT 0.00,
  PRIMARY KEY (`MaVatTu`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of vattu
-- ----------------------------
INSERT INTO `vattu` VALUES ('VT01', 'Thép cuộn xây dựng', 'Kg', 10500.00);
INSERT INTO `vattu` VALUES ('VT02', 'Xi măng PCB40', 'Tấn', 300.00);
INSERT INTO `vattu` VALUES ('VT03', 'Gạch thẻ đặc', 'Viên', 1000.00);

SET FOREIGN_KEY_CHECKS = 1;
