SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for mtags
-- ----------------------------
DROP TABLE IF EXISTS `mtags`;
CREATE TABLE `mtags` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `photoId` int(11) NOT NULL,
  `name` varchar(30) NOT NULL,
  `score` float(11,9) NOT NULL,
  `source` varchar(30) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `id` (`id`) USING BTREE,
  KEY `photoId` (`photoId`),
  CONSTRAINT `mtags_ibfk_1` FOREIGN KEY (`photoId`) REFERENCES `photos` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
