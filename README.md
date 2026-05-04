# PBL3_FastFood

// Cơ bản admin đã xong, coi test các chức năng và feedback để sửa

// Trang Hóa Đơn chưa sửa
// Trang Bán Hàng, Mua Hàng test, UI
// Update lại dữ liệu có ràng buộc dễ đọc

## Kết luận
Trong phạm vi môn học, nhóm đã xây dựng được một hệ thống quản lý cửa hàng đồ ăn nhanh với các chức năng cốt lõi như quản lý món, quản lý đơn hàng, nhập hàng, quản lý khách hàng và nhân viên, cùng các màn hình thao tác cho nghiệp vụ bán hàng.

Quá trình triển khai giúp nhóm hiểu rõ hơn về cách tổ chức kiến trúc theo tầng (UI, Business, DataAccess), cách kết nối và thao tác cơ sở dữ liệu, cũng như cách thiết kế luồng nghiệp vụ phù hợp với thực tế cửa hàng. Dù còn nhiều phần cần hoàn thiện, sản phẩm hiện tại đã thể hiện được quy trình vận hành chính và có thể dùng để kiểm thử, lấy phản hồi từ người dùng.

Nhóm rút ra rằng việc phân chia công việc, thống nhất quy ước đặt tên và quy trình làm việc (review, test, cập nhật dữ liệu mẫu) ảnh hưởng rất lớn đến chất lượng sản phẩm.

Một số hạn chế hiện tại là giao diện chưa đồng nhất hoàn toàn, một vài màn hình còn đang sửa (như Hóa đơn), và dữ liệu ràng buộc chưa được chuẩn hóa đầy đủ.

Tuy vậy, dự án là nền tảng tốt để nhóm tiếp tục phát triển, mở rộng tính năng và hoàn thiện trải nghiệm người dùng.

## Hướng phát triển
Trong thời gian tới, nhóm định hướng cải tiến theo các hướng sau để sản phẩm thực tế hơn và dễ triển khai:
- **Hoàn thiện UI/UX**: đồng bộ giao diện, tối ưu luồng thao tác bán hàng, thêm thông báo rõ ràng khi lỗi hoặc thao tác thành công.
- **Bổ sung chức năng nghiệp vụ**: hoàn thiện trang Hóa đơn, thêm quản lý khuyến mãi, hoàn/hủy đơn, theo dõi tồn kho theo lô và hạn sử dụng.
- **Báo cáo & thống kê**: xây dựng dashboard doanh thu theo ngày/tuần/tháng, top món bán chạy, hiệu quả nhân viên, hỗ trợ xuất báo cáo.
- **Bảo mật & phân quyền**: phân quyền chi tiết theo vai trò (quản lý, thu ngân, kho), log thao tác và cảnh báo khi dữ liệu bị thay đổi bất thường.
- **Tối ưu dữ liệu**: chuẩn hóa dữ liệu ràng buộc, thêm kiểm tra tính hợp lệ khi nhập, đồng bộ dữ liệu mẫu để dễ demo và test.
- **Khả năng mở rộng**: nghiên cứu tích hợp thanh toán điện tử, kết nối máy in hóa đơn, và hướng tới phiên bản web/mobile để sử dụng linh hoạt hơn.
