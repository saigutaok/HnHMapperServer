.mode column
.headers on
SELECT 'Maps Table:' as Info;
SELECT * FROM Maps;
SELECT '' as blank;
SELECT 'Grids Count:' as Info;
SELECT COUNT(*) as Count, Map FROM Grids GROUP BY Map;
SELECT '' as blank;
SELECT 'Tiles Count by Map and Zoom:' as Info;
SELECT MapId, Zoom, COUNT(*) as Count FROM Tiles GROUP BY MapId, Zoom ORDER BY MapId, Zoom;
SELECT '' as blank;
SELECT 'Sample Tiles:' as Info;
SELECT MapId, Zoom, CoordX, CoordY, File FROM Tiles LIMIT 10;
