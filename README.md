# PBL3_FastFood
Hash password sài BCrypt.Net (cài vào nugget C#)
Nếu các mật khẩu ở SQLSEVER vẫn là 123

UPDATE dbo.NHAN_VIEN 
SET MatKhau = '$2a$11$Gp2jEU5jKnlNfQmknP0Sm.G/PvZS1gF8Zu6gj7qS9RXlHBiVNBvQ2'
WHERE MatKhau = '123';
