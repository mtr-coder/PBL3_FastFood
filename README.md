# PBL3_FastFood

Tạm thời OK, test thêm có lỗi thì fix
Xây lại dữ liệu database

Hash password sài BCrypt.Net (cài vào nugget C#)
Nếu các mật khẩu ở SQLSEVER vẫn là 123,1234 thì chạy 2 lệnh này trong sql

UPDATE dbo.NHAN_VIEN 
SET MatKhau = '$2a$11$Gp2jEU5jKnlNfQmknP0Sm.G/PvZS1gF8Zu6gj7qS9RXlHBiVNBvQ2'
WHERE MatKhau = '123';

-- Update password "1234" → Hash BCrypt  
UPDATE dbo.NHAN_VIEN 
SET MatKhau = '$2a$11$4aDhyynDMlFQVVNkNKqbZ.0vNmPpvPmOy/8jNIq7m3TBnlqIBQH5G'
WHERE MatKhau = '1234';


