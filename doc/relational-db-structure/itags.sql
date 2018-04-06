/*
Navicat MySQL Data Transfer

Source Server         : Hetzner
Source Server Version : 50558
Source Host           : 78.46.178.185:3306
Source Database       : instatagger

Target Server Type    : MYSQL
Target Server Version : 50558
File Encoding         : 65001

Date: 2018-03-28 18:40:27
*/

SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for itags
-- ----------------------------
DROP TABLE IF EXISTS `itags`;
CREATE TABLE `itags` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `photoId` int(11) NOT NULL,
  `value` varchar(30) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `photoId` (`photoId`),
  CONSTRAINT `itags_ibfk_1` FOREIGN KEY (`photoId`) REFERENCES `photos` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8;
