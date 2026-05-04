using PBL3.Business;
using System.Data;
using System.IO;
using System.Globalization;

namespace PBL3
{
    public partial class QuanLiMonAn : Form
    {
        private readonly QuanLiMonAnService _quanLiMonAnService;
        private DataTable? _monAnTable;
        private string? _selectedMaMonDbValue;
        private bool _addedThumbnailColumn;
        private readonly string _monAnImageFolder;
        private string? _pendingImagePath;
        private readonly ErrorProvider _errorProvider;

        private DataTable _sizeTable = new DataTable();
        private DataTable _dinhMucTable = new DataTable();
        private DataTable? _nguyenLieuLookup;

        private TabControl? _tabChiTiet;
        private DataGridView? _dgvGiaBan;
        private DataGridView? _dgvDinhMuc;
        private ComboBox? _cboDvpvChiTiet;
        private TextBox? _txtDonGiaSize;
        private Button? _btnThemSize;
        private Button? _btnXoaSize;
        private ComboBox? _cboNguyenLieu;
        private TextBox? _txtSoLuongDinhMuc;
        private Button? _btnThemDinhMuc;
        private Button? _btnXoaDinhMuc;
        private Label? _lblGiaVon;
        private readonly BindingSource _dinhMucBindingSource = new BindingSource();
        private string _currentMaMonDetail = string.Empty;

        public QuanLiMonAn()
        {
            _quanLiMonAnService = new QuanLiMonAnService();
            InitializeComponent();
            _cboTimTheo.SelectionChangeCommitted += SearchControl_Changed;
            _cboMaLoai.SelectionChangeCommitted += CboMaLoai_SelectionChangeCommitted;
            _cboMaLoai.DoubleClick += CboMaLoai_DoubleClick;
            _dgvDinhMuc.DataBindingComplete += DgvDinhMuc_DataBindingComplete;
            _cboMaDvt.SelectionChangeCommitted += CboMaDvt_SelectionChangeCommitted;
            _monAnImageFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MonAnImages");
            Directory.CreateDirectory(_monAnImageFolder);

            _errorProvider = new ErrorProvider { BlinkStyle = ErrorBlinkStyle.NeverBlink };
            InitializeDetailTables();
        }

        private void QuanLiMonAn_Load(object? sender, EventArgs e)
        {
            try
            {
                LoadLoaiMon();
                LoadDonViTinh();
                LoadDonViPhucVuOptions();
                LoadNguyenLieuOptions();
                LoadTrangThai();
                LoadMonAn();
                if (_cboTimTheo.Items.Count > 0 && _cboTimTheo.SelectedIndex < 0)
                {
                    _cboTimTheo.SelectedIndex = 0;
                }

                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể tải dữ liệu món ăn.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvDinhMuc_DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
        {
            ConfigureDinhMucGridColumns();
        }

        private void LoadMonAn()
        {
            _monAnTable = _quanLiMonAnService.GetMonAn();
            _dgvMonAn.DataSource = _monAnTable;
            _dgvMonAn.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            _dgvMonAn.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            _dgvMonAn.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

            EnsureThumbnailColumn();

            SetHeaderText("MaMon", "Mã món");
            SetHeaderText("AnhMon", "Ảnh");
            SetHeaderText("TenMon", "Tên món");
            SetHeaderText("MaLoai", "Mã loại");
            SetHeaderText("TenLoai", "Tên loại");
            SetHeaderText("MaDVT", "Mã DVT");
            SetHeaderText("TenDVT", "Tên DVT");
            SetHeaderText("TrangThai", "Trạng thái");

            SetColumnWidth("MaMon", 95);
            SetColumnWidth("AnhMon", 70);
            SetColumnWidth("TenMon", 175);
            SetColumnWidth("TenLoai", 90);
            SetColumnWidth("TrangThai", 100);

            DataGridViewColumn? maLoaiColumn = _dgvMonAn.Columns["MaLoai"];
            if (maLoaiColumn is not null)
            {
                maLoaiColumn.Visible = false;
            }

            DataGridViewColumn? maDvtColumn = _dgvMonAn.Columns["MaDVT"];
            if (maDvtColumn is not null)
            {
                maDvtColumn.Visible = false;
            }

            DataGridViewColumn? tenDvtColumn = _dgvMonAn.Columns["TenDVT"];
            if (tenDvtColumn is not null)
            {
                tenDvtColumn.Visible = false;
            }

            DataGridViewColumn? anhMonColumn = _dgvMonAn.Columns["AnhMon"];
            if (anhMonColumn is DataGridViewImageColumn imageColumn)
            {
                imageColumn.ImageLayout = DataGridViewImageCellLayout.Zoom;
            }

            _dgvMonAn.RowTemplate.Height = 52;

            ApplySearchFilter();
            FillThumbnailValues();
        }

        private void SearchControl_Changed(object? sender, EventArgs e)
        {
            ApplySearchFilter();
            FillThumbnailValues();
        }

        private void CboMaLoai_DoubleClick(object? sender, EventArgs e)
        {
            using (QuanLiLoaiMon formLoaiMon = new QuanLiLoaiMon())
            {
                formLoaiMon.ShowDialog();
            }
            LoadLoaiMon();
        }

        private void CboMaLoai_SelectionChangeCommitted(object? sender, EventArgs e)
        {
            if (_cboMaLoai.SelectedValue?.ToString() == "-1")
            {
                using (QuanLiLoaiMon formLoaiMon = new QuanLiLoaiMon())
                {
                    formLoaiMon.ShowDialog();
                }
                LoadLoaiMon();

                if (_cboMaLoai.Items.Count > 0)
                {
                    _cboMaLoai.SelectedIndex = 0;
                }
            }
        }

        private void ApplySearchFilter()
        {
            if (_monAnTable is null)
            {
                return;
            }

            string keyword = _txtTimKiem.Text.Trim().Replace("'", "''");
            string selectedMaLoai = _cboTimTheo.SelectedValue?.ToString() ?? "-99";

            List<string> filterConditions = new List<string>();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                filterConditions.Add($"(Convert(MaMon, 'System.String') LIKE '%{keyword}%' OR TenMon LIKE '%{keyword}%')");
            }
            if (selectedMaLoai != "-99" && !string.IsNullOrEmpty(selectedMaLoai))
            {
                filterConditions.Add($"Convert(MaLoai, 'System.String') = '{selectedMaLoai}'");
            }
            if (filterConditions.Count > 0)
            {
                _monAnTable.DefaultView.RowFilter = string.Join(" AND ", filterConditions);
            }
            else
            {
                _monAnTable.DefaultView.RowFilter = string.Empty;
            }
        }

        private void SetColumnWidth(string columnName, int width)
        {
            DataGridViewColumn? column = _dgvMonAn.Columns[columnName];
            if (column is not null)
            {
                column.Width = width;
            }
        }

        private void SetHeaderText(string columnName, string headerText)
        {
            DataGridViewColumn? column = _dgvMonAn.Columns[columnName];
            if (column is not null)
            {
                column.HeaderText = headerText;
            }
        }

        private void EnsureThumbnailColumn()
        {
            if (_addedThumbnailColumn)
            {
                return;
            }

            if (!_dgvMonAn.Columns.Contains("AnhMon"))
            {
                DataGridViewImageColumn imageColumn = new DataGridViewImageColumn
                {
                    Name = "AnhMon",
                    HeaderText = "Ảnh",
                    Width = 70,
                    ImageLayout = DataGridViewImageCellLayout.Zoom
                };

                _dgvMonAn.Columns.Insert(1, imageColumn);
            }

            _addedThumbnailColumn = true;
        }

        private void FillThumbnailValues()
        {
            if (_dgvMonAn.Rows.Count == 0)
            {
                return;
            }

            foreach (DataGridViewRow row in _dgvMonAn.Rows)
            {
                string maMon = Convert.ToString(row.Cells["MaMon"].Value) ?? string.Empty;
                row.Cells["AnhMon"].Value = GetMonThumbnail(maMon);
            }
        }

        private Image GetMonThumbnail(string maMon)
        {
            if (string.IsNullOrWhiteSpace(maMon))
            {
                return CreateEmptyThumbnail();
            }

            string[] imageKeys = GetMonImageKeys(maMon);
            string[] extensions = [".jpg", ".jpeg", ".png", ".bmp", ".gif"];
            string[] candidates = imageKeys
                .SelectMany(k => extensions.Select(ext => Path.Combine(_monAnImageFolder, $"{k}{ext}")))
                .ToArray();

            string? imagePath = candidates.FirstOrDefault(File.Exists);
            if (imagePath is null)
            {
                return CreateEmptyThumbnail();
            }

            using FileStream stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
            using Image img = Image.FromStream(stream);
            return new Bitmap(img, new Size(48, 48));
        }

        private static Bitmap CreateEmptyThumbnail()
        {
            Bitmap bmp = new Bitmap(48, 48);
            using Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.WhiteSmoke);
            using Pen p = new Pen(Color.Gainsboro);
            g.DrawRectangle(p, 0, 0, 47, 47);
            return bmp;
        }

        private void DgvNhanVien_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            DataGridViewRow row = _dgvMonAn.Rows[e.RowIndex];
            _selectedMaMonDbValue = Convert.ToString(row.Cells["MaMon"].Value) ?? string.Empty;
            _txtMaMA.Text = FormatMaMonForDisplay(_selectedMaMonDbValue);
            _txtTenMon.Text = Convert.ToString(row.Cells["TenMon"].Value) ?? string.Empty;
            _cboMaLoai.SelectedValue = Convert.ToString(row.Cells["MaLoai"].Value) ?? string.Empty;
            _cboMaDvt.SelectedValue = Convert.ToString(row.Cells["MaDVT"].Value) ?? string.Empty;
            _cboTrangThaiMon.SelectedItem = Convert.ToString(row.Cells["TrangThai"].Value) ?? "Đang bán";

            string maMon = _selectedMaMonDbValue ?? _txtMaMA.Text.Trim();
            LoadMonImage(maMon);
            LoadMonDetails(maMon);
        }

        private bool ValidateInput(bool isInsert)
        {
            if (!isInsert && string.IsNullOrWhiteSpace(_txtMaMA.Text))
            {
                MessageBox.Show("Vui lòng chọn món ăn.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(_txtTenMon.Text))
            {
                _errorProvider.SetError(_txtTenMon, "Vui lòng nhập tên món.");
                return false;
            }
            _errorProvider.SetError(_txtTenMon, string.Empty);

            if (_cboMaLoai.SelectedValue is null || _cboMaLoai.SelectedValue?.ToString() == "-1")
            {
                MessageBox.Show("Vui lòng chọn loại món hợp lệ.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (_cboMaDvt.SelectedValue is null)
            {
                MessageBox.Show("Vui lòng nhập mã đơn vị tính.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(Convert.ToString(_cboTrangThaiMon.SelectedItem)))
            {
                MessageBox.Show("Vui lòng chọn trạng thái.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!ForeignKeysExist())
            {
                MessageBox.Show("Mã loại hoặc mã đơn vị tính không tồn tại trong danh mục.", "Dữ liệu sai", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void BtnThem_Click(object? sender, EventArgs e)
        {
            if (!ValidateInput(true))
            {
                return;
            }
            try
            {
                string maMonDbValue = _quanLiMonAnService.InsertMonAn(
                    _txtTenMon.Text,
                    _cboMaLoai.SelectedValue?.ToString() ?? string.Empty,
                    _cboMaDvt.SelectedValue?.ToString() ?? string.Empty,
                    Convert.ToString(_cboTrangThaiMon.SelectedItem) ?? "Đang bán",
                    _tabChiTiet?.Visible == true ? _sizeTable : new DataTable(),
                    _tabChiTiet?.Visible == true ? _dinhMucTable : new DataTable());

                SavePendingImage(maMonDbValue);

                MessageBox.Show("Thêm món ăn thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadMonAn();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Thêm món ăn thất bại.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSua_Click(object? sender, EventArgs e)
        {
            if (!ValidateInput(false))
            {
                return;
            }

            try
            {
                string maMonDbValue = _selectedMaMonDbValue ?? _txtMaMA.Text.Trim();
                int rows = _quanLiMonAnService.UpdateMonAn(
                    maMonDbValue,
                    _txtTenMon.Text,
                    _cboMaLoai.SelectedValue?.ToString() ?? string.Empty,
                    _cboMaDvt.SelectedValue?.ToString() ?? string.Empty,
                    Convert.ToString(_cboTrangThaiMon.SelectedItem) ?? "Đang bán",
                    _tabChiTiet?.Visible == true ? _sizeTable : new DataTable(),
                    _tabChiTiet?.Visible == true ? _dinhMucTable : new DataTable());

                if (rows == 0)
                {
                    MessageBox.Show("Không tìm thấy món ăn để cập nhật.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SavePendingImage(maMonDbValue);

                MessageBox.Show("Cập nhật món ăn thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadMonAn();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cập nhật món ăn thất bại.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnXoa_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtMaMA.Text))
            {
                MessageBox.Show("Vui lòng chọn món ăn cần xóa.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show(
                "Bạn có chắc chắn muốn xóa món ăn này?",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                return;
            }

            try
            {
                int rows = _quanLiMonAnService.DeleteMonAn(_selectedMaMonDbValue ?? _txtMaMA.Text.Trim());

                if (rows == 0)
                {
                    MessageBox.Show("Không tìm thấy món ăn để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show("Xóa món ăn thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadMonAn();
                ClearForm();
            }
            catch (Exception ex) when (DataErrorHelper.IsForeignKeyViolation(ex))
            {
                MessageBox.Show("Không thể xóa món ăn vì đang được sử dụng ở dữ liệu liên quan.", "Không thể xóa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Xóa món ăn thất bại.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnLamMoi_Click(object? sender, EventArgs e)
        {
            ClearForm();
            LoadMonAn();
        }

        private void ClearForm()
        {
            _selectedMaMonDbValue = null;
            _txtMaMA.Text = GenerateNextMaMon();
            _txtTenMon.Clear();
            if (_cboMaLoai.Items.Count > 0)
            {
                _cboMaLoai.SelectedIndex = 0;
            }

            if (_cboMaDvt.Items.Count > 0)
            {
                _cboMaDvt.SelectedIndex = 0;
            }

            _cboTrangThaiMon.SelectedItem = "Đang bán";
            _pendingImagePath = null;
            _picAnhMon.Image = null;
            _sizeTable.Rows.Clear();
            _dinhMucTable.Rows.Clear();
            RefreshDetailBindings();
            _txtTenMon.Focus();
        }

        private void BtnChonAnhMon_Click(object? sender, EventArgs e)
        {
            using OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Chọn ảnh món ăn",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
                Multiselect = false
            };

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            _pendingImagePath = dialog.FileName;
            LoadImageToPreview(_pendingImagePath);
        }

        private void SavePendingImage(string maMon)
        {
            if (string.IsNullOrWhiteSpace(maMon) || string.IsNullOrWhiteSpace(_pendingImagePath) || !File.Exists(_pendingImagePath))
            {
                return;
            }

            string extension = Path.GetExtension(_pendingImagePath);
            DeleteExistingMonImages(maMon);
            string destination = Path.Combine(_monAnImageFolder, $"{maMon}{extension}");
            File.Copy(_pendingImagePath, destination, true);
            _pendingImagePath = null;
        }

        private void DeleteExistingMonImages(string maMon)
        {
            string[] imageKeys = GetMonImageKeys(maMon);
            string[] extensions = [".jpg", ".jpeg", ".png", ".bmp", ".gif"];
            foreach (string fileName in imageKeys.SelectMany(k => extensions.Select(ext => $"{k}{ext}")))
            {
                string filePath = Path.Combine(_monAnImageFolder, fileName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }

        private void LoadMonImage(string maMon)
        {
            if (string.IsNullOrWhiteSpace(maMon))
            {
                _picAnhMon.Image = null;
                return;
            }

            string[] imageKeys = GetMonImageKeys(maMon);
            string[] extensions = [".jpg", ".jpeg", ".png", ".bmp", ".gif"];
            string[] candidates = imageKeys
                .SelectMany(k => extensions.Select(ext => Path.Combine(_monAnImageFolder, $"{k}{ext}")))
                .ToArray();

            string? imagePath = candidates.FirstOrDefault(File.Exists);
            if (imagePath is null)
            {
                _picAnhMon.Image = null;
                return;
            }
            LoadImageToPreview(imagePath);
        }

        private static string[] GetMonImageKeys(string maMon)
        {
            string value = maMon.Trim();
            List<string> keys = new List<string> { value };

            if (value.StartsWith("MA", StringComparison.OrdinalIgnoreCase))
            {
                string numericPart = value[2..];
                if (!string.IsNullOrWhiteSpace(numericPart))
                {
                    keys.Add(numericPart);
                }
            }
            else
            {
                keys.Add($"MA{value}");
            }

            return keys.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        }

        private void LoadImageToPreview(string imagePath)
        {
            using FileStream stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
            using Image img = Image.FromStream(stream);
            _picAnhMon.Image = new Bitmap(img);
        }

        private void LoadLoaiMon()
        {
            DataTable dt = _quanLiMonAnService.GetLoaiMonOptions();
            _cboMaLoai.DataSource = dt;
            _cboMaLoai.DisplayMember = "TenLoai";
            _cboMaLoai.ValueMember = "MaLoai";
            DataTable dtFilter = dt.Copy();
            DataRow[] editRows = dtFilter.Select("Convert(MaLoai, 'System.String') = '-1'");
            foreach (DataRow row in editRows)
            {
                dtFilter.Rows.Remove(row);
            }
            DataRow rowAll = dtFilter.NewRow();
            rowAll["MaLoai"] = "-99";
            rowAll["TenLoai"] = "Tất cả";
            dtFilter.Rows.InsertAt(rowAll, 0);
            _cboTimTheo.SelectionChangeCommitted -= SearchControl_Changed;

            _cboTimTheo.DataSource = dtFilter;
            _cboTimTheo.DisplayMember = "TenLoai";
            _cboTimTheo.ValueMember = "MaLoai";

            _cboTimTheo.SelectionChangeCommitted += SearchControl_Changed;
        }

        private void LoadDonViTinh()
        {
            DataTable dt = _quanLiMonAnService.GetDonViTinhOptions();
            _cboMaDvt.DataSource = dt;
            _cboMaDvt.DisplayMember = "TenDVT";
            _cboMaDvt.ValueMember = "MaDVT";
        }
        private void CboMaDvt_SelectionChangeCommitted(object? sender, EventArgs e)
        {
            if (_cboMaDvt.SelectedValue?.ToString() == "-1")
            {
                using (QuanLiDonViTinh formDvt = new QuanLiDonViTinh())
                {
                    formDvt.ShowDialog();
                }
                LoadDonViTinh();

                if (_cboMaDvt.Items.Count > 0)
                {
                    _cboMaDvt.SelectedIndex = 0;
                }
            }
        }
        private void LoadTrangThai()
        {
            _cboTrangThaiMon.Items.Clear();
            _cboTrangThaiMon.Items.Add("Đang bán");
            _cboTrangThaiMon.Items.Add("Ngừng bán");
            _cboTrangThaiMon.SelectedIndex = 0;
        }

        private void InitializeDetailTables()
        {
            _sizeTable.Columns.Add("MaDVPV", typeof(string));
            _sizeTable.Columns.Add("TenDVPV", typeof(string));
            _sizeTable.Columns.Add("DonGia", typeof(decimal));
            _sizeTable.Columns.Add("TrangThai", typeof(string));

            _dinhMucTable.Columns.Add("MaDVPV", typeof(string));
            _dinhMucTable.Columns.Add("MaNL", typeof(string));
            _dinhMucTable.Columns.Add("TenNL", typeof(string));
            _dinhMucTable.Columns.Add("SoLuongSuDung", typeof(decimal));
            _dinhMucTable.Columns.Add("DonViTinh", typeof(string));
            _dinhMucTable.Columns.Add("GiaNhap", typeof(decimal));
            _dinhMucTable.Columns.Add("SoLuongTon", typeof(decimal));
            _dinhMucTable.Columns.Add("ThanhTien", typeof(decimal), "SoLuongSuDung * GiaNhap");

            RefreshDetailBindings();
        }

        private void RefreshDetailBindings()
        {
            _dgvGiaBan!.DataSource = _sizeTable;
            _dinhMucBindingSource.DataSource = _dinhMucTable;
            _dgvDinhMuc!.DataSource = _dinhMucBindingSource;

            if (_dgvGiaBan != null)
            {
                _dgvGiaBan.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                _dgvGiaBan.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
                _dgvGiaBan.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            }

            if (_dgvDinhMuc != null)
            {
                _dgvDinhMuc.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                _dgvDinhMuc.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
                _dgvDinhMuc.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            }

            ConfigureGiaBanGridColumns();
            ConfigureDinhMucGridColumns();

            DataGridViewColumn? maDvpvDinhMucColumn = _dgvDinhMuc.Columns["MaDVPV"];
            if (maDvpvDinhMucColumn is not null)
            {
                maDvpvDinhMucColumn.Visible = false;
            }

            DataGridViewColumn? trangThaiColumn = _dgvGiaBan.Columns["TrangThai"];
            if (trangThaiColumn is not null)
            {
                trangThaiColumn.Visible = false;
            }

            if (_dgvGiaBan.Rows.Count > 0 && _dgvGiaBan.CurrentCell is null)
            {
                DataGridViewCell? firstVisibleCell = GetFirstVisibleGiaBanCell(_dgvGiaBan.Rows[0]);
                if (firstVisibleCell is not null)
                {
                    _dgvGiaBan.CurrentCell = firstVisibleCell;
                    _dgvGiaBan.Rows[0].Selected = true;
                }
            }

            // Apply fixed height to existing rows in detail grids to avoid oversized cells
            if (_dgvGiaBan is not null)
            {
                foreach (DataGridViewRow r in _dgvGiaBan.Rows)
                {
                    r.Height = _dgvGiaBan.RowTemplate.Height;
                }
            }

            if (_dgvDinhMuc is not null)
            {
                foreach (DataGridViewRow r in _dgvDinhMuc.Rows)
                {
                    r.Height = _dgvDinhMuc.RowTemplate.Height;
                }
            }

            ApplyDinhMucFilterAndGiaVon();
        }

        private void ConfigureGiaBanGridColumns()
        {
            if (_dgvGiaBan is null)
            {
                return;
            }

            SetGiaBanColumn("TenDVPV", "Tên loại phục vụ", visible: true, width: 170);
            SetGiaBanColumn("DonGia", "Đơn giá", visible: true, width: 120, format: "N0");

            SetGiaBanColumn("MaDVPV", visible: false);
            SetGiaBanColumn("TrangThai", visible: false);
        }

        private void SetGiaBanColumn(string columnName, string? header = null, bool visible = true, int? width = null, string? format = null)
        {
            DataGridViewColumn? column = _dgvGiaBan?.Columns[columnName];
            if (column is null)
            {
                return;
            }

            column.Visible = visible;
            if (!visible)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(header))
            {
                column.HeaderText = header;
            }

            if (width.HasValue)
            {
                column.Width = width.Value;
            }

            if (!string.IsNullOrWhiteSpace(format))
            {
                column.DefaultCellStyle.Format = format;
            }
        }

        private void ConfigureDinhMucGridColumns()
        {
            if (_dgvDinhMuc is null)
            {
                return;
            }

            SetDinhMucColumn("TenNL", "Tên nguyên liệu", visible: true, width: 180);
            SetDinhMucColumn("SoLuongSuDung", "Số lượng", visible: true, width: 100, format: "N2");
            SetDinhMucColumn("DonViTinh", "Đơn vị", visible: true, width: 65);
            SetDinhMucColumn("ThanhTien", "Thành tiền", visible: true, width: 110, format: "N0");

            SetDinhMucColumn("MaDVPV", visible: false);
            SetDinhMucColumn("MaNL", visible: false);
            SetDinhMucColumn("GiaNhap", visible: false);
            SetDinhMucColumn("SoLuongTon", visible: false);

            DataGridViewColumn? maDvpvColumn = _dgvDinhMuc.Columns
                .Cast<DataGridViewColumn>()
                .FirstOrDefault(c =>
                    string.Equals(c.Name, "MaDVPV", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(c.DataPropertyName, "MaDVPV", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(c.HeaderText, "MaDVPV", StringComparison.OrdinalIgnoreCase));

            if (maDvpvColumn is not null)
            {
                maDvpvColumn.Visible = false;
            }
        }

        private void SetDinhMucColumn(string columnName, string? header = null, bool visible = true, int? width = null, string? format = null)
        {
            DataGridViewColumn? column = _dgvDinhMuc?.Columns[columnName];
            if (column is null)
            {
                return;
            }

            column.Visible = visible;
            if (!visible)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(header))
            {
                column.HeaderText = header;
            }

            if (width.HasValue)
            {
                column.Width = width.Value;
            }

            if (!string.IsNullOrWhiteSpace(format))
            {
                column.DefaultCellStyle.Format = format;
            }
        }

        private void LoadDonViPhucVuOptions()
        {
            DataTable dt = _quanLiMonAnService.GetDonViPhucVuOptions();
            _cboDvpvChiTiet!.DataSource = dt;
            _cboDvpvChiTiet.DisplayMember = "TenDVPV";
            _cboDvpvChiTiet.ValueMember = "MaDVPV";
        }

        private void LoadNguyenLieuOptions()
        {
            DataTable dt = _quanLiMonAnService.GetNguyenLieuOptions();
            _nguyenLieuLookup = dt;
            _cboNguyenLieu!.DataSource = dt.Copy();
            _cboNguyenLieu.DisplayMember = "TenNL";
            _cboNguyenLieu.ValueMember = "MaNL";
        }

        private void LoadMonDetails(string maMon)
        {
            _currentMaMonDetail = maMon;
            _sizeTable.Rows.Clear();
            _dinhMucTable.Rows.Clear();
            if (string.IsNullOrWhiteSpace(maMon))
            {
                RefreshDetailBindings();
                return;
            }

            var details = _quanLiMonAnService.GetMonDetails(maMon);
            _sizeTable = details.sizeTable;
            _dinhMucTable = details.dinhMucTable;

            RefreshDetailBindings();
            if (_dgvGiaBan!.Rows.Count > 0)
            {
                int preferredIndex = 0;
                for (int i = 0; i < _dgvGiaBan.Rows.Count; i++)
                {
                    string maDvpv = Convert.ToString(_dgvGiaBan.Rows[i].Cells["MaDVPV"].Value) ?? string.Empty;
                    bool hasDinhMuc = _dinhMucTable.AsEnumerable().Any(r => string.Equals(Convert.ToString(r["MaDVPV"]), maDvpv, StringComparison.OrdinalIgnoreCase));
                    if (hasDinhMuc)
                    {
                        preferredIndex = i;
                        break;
                    }
                }

                _dgvGiaBan.ClearSelection();
                DataGridViewCell? firstVisibleCell = GetFirstVisibleGiaBanCell(_dgvGiaBan.Rows[preferredIndex]);
                if (firstVisibleCell is not null)
                {
                    _dgvGiaBan.CurrentCell = firstVisibleCell;
                    _dgvGiaBan.Rows[preferredIndex].Selected = true;
                }
                ApplyDinhMucFilterAndGiaVon();
            }
        }

        private static DataGridViewCell? GetFirstVisibleGiaBanCell(DataGridViewRow row)
        {
            foreach (DataGridViewCell cell in row.Cells)
            {
                if (cell.OwningColumn.Visible)
                {
                    return cell;
                }
            }

            return null;
        }

        private void DgvGiaBan_SelectionChanged(object? sender, EventArgs e)
        {
            ApplyDinhMucFilterAndGiaVon();
        }

        private void ApplyDinhMucFilterAndGiaVon()
        {
            if (_dgvGiaBan is null || _lblGiaVon is null)
            {
                return;
            }

            string? maDvpv = GetSelectedMaDvpv();
            if (string.IsNullOrWhiteSpace(maDvpv))
            {
                _dinhMucBindingSource.Filter = "1 = 0";
                _lblGiaVon.Text = "Chi phí ước tính: 0 đ";
                return;
            }

            string escaped = maDvpv.Replace("'", "''");
            _dinhMucBindingSource.Filter = $"MaDVPV = '{escaped}'";

            bool hasAnyForSelectedSize = _dinhMucTable.AsEnumerable()
                .Any(r => string.Equals(Convert.ToString(r["MaDVPV"]), maDvpv, StringComparison.OrdinalIgnoreCase));

            if (!hasAnyForSelectedSize)
            {
                _dinhMucBindingSource.Filter = string.Empty;
            }

            decimal giaVon = _dinhMucTable.AsEnumerable()
                .Where(r => !hasAnyForSelectedSize || string.Equals(Convert.ToString(r["MaDVPV"]), maDvpv, StringComparison.OrdinalIgnoreCase))
                .Sum(r => ToDecimalOrZero(r["SoLuongSuDung"]) * ToDecimalOrZero(r["GiaNhap"]));

            _lblGiaVon.Text = $"Chi phí ước tính: {giaVon:N0} đ";

            bool outOfStock = _dinhMucTable.AsEnumerable()
                .Where(r => !hasAnyForSelectedSize || string.Equals(Convert.ToString(r["MaDVPV"]), maDvpv, StringComparison.OrdinalIgnoreCase))
                .Any(r => ToDecimalOrZero(r["SoLuongTon"]) <= 0m);

            if (outOfStock)
            {
                _cboTrangThaiMon.SelectedItem = "Ngừng bán";
                _lblGiaVon.ForeColor = Color.Firebrick;
            }
            else
            {
                _lblGiaVon.ForeColor = Color.Black;
            }
        }

        private string? GetSelectedMaDvpv()
        {
            if (_dgvGiaBan is null || _dgvGiaBan.Rows.Count == 0)
            {
                return null;
            }

            if (_dgvGiaBan.SelectedRows.Count > 0)
            {
                return Convert.ToString(_dgvGiaBan.SelectedRows[0].Cells["MaDVPV"].Value);
            }

            if (_dgvGiaBan.CurrentRow?.Cells["MaDVPV"].Value is not null)
            {
                return Convert.ToString(_dgvGiaBan.CurrentRow.Cells["MaDVPV"].Value);
            }

            return Convert.ToString(_dgvGiaBan.Rows[0].Cells["MaDVPV"].Value);
        }

        private void BtnThemSize_Click(object? sender, EventArgs e)
        {
            _errorProvider.SetError(_txtDonGiaSize!, string.Empty);

            string maDvpv = _cboDvpvChiTiet?.SelectedValue?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(maDvpv))
            {
                return;
            }

            if (!decimal.TryParse(_txtDonGiaSize!.Text.Trim(), out decimal donGia) || donGia <= 0)
            {
                _errorProvider.SetError(_txtDonGiaSize, "Đơn giá không hợp lệ.");
                return;
            }

            bool exists = _sizeTable.AsEnumerable().Any(r => string.Equals(Convert.ToString(r["MaDVPV"]), maDvpv, StringComparison.OrdinalIgnoreCase));
            if (exists)
            {
                MessageBox.Show("Size này đã tồn tại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string tenDvpv = (_cboDvpvChiTiet.SelectedItem as DataRowView)?["TenDVPV"]?.ToString() ?? maDvpv;
            _sizeTable.Rows.Add(maDvpv, tenDvpv, donGia, Convert.ToString(_cboTrangThaiMon.SelectedItem) ?? "Đang bán");
            _txtDonGiaSize.Clear();
            RefreshDetailBindings();
        }

        private void BtnXoaSize_Click(object? sender, EventArgs e)
        {
            string? maDvpv = GetSelectedMaDvpv();
            if (string.IsNullOrWhiteSpace(maDvpv))
            {
                return;
            }
            // Xác nhận xóa size
            if (MessageBox.Show("Bạn có chắc chắn muốn xóa size này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            // Xác nhận xóa định mức
            if (MessageBox.Show("Bạn có chắc chắn muốn xóa định mức này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            string maMon = _currentMaMonDetail;
            if (!string.IsNullOrWhiteSpace(maMon))
            {
                try
                {
                    _quanLiMonAnService.DeleteSizeAndDinhMuc(maMon, maDvpv);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa size ở DB: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // Remove from in-memory tables
            foreach (DataRow row in _dinhMucTable.Select($"MaDVPV = '{maDvpv.Replace("'", "''")}'"))
            {
                row.Delete();
            }

            foreach (DataRow row in _sizeTable.Select($"MaDVPV = '{maDvpv.Replace("'", "''")}'"))
            {
                row.Delete();
            }

            _dinhMucTable.AcceptChanges();
            _sizeTable.AcceptChanges();
            RefreshDetailBindings();
        }

        private void BtnThemDinhMuc_Click(object? sender, EventArgs e)
        {
            _errorProvider.SetError(_txtSoLuongDinhMuc!, string.Empty);

            string? maDvpv = GetSelectedMaDvpv();
            if (string.IsNullOrWhiteSpace(maDvpv))
            {
                MessageBox.Show("Vui lòng chọn size trước khi thêm định mức.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(_txtSoLuongDinhMuc!.Text.Trim(), out decimal soLuong) || soLuong <= 0)
            {
                _errorProvider.SetError(_txtSoLuongDinhMuc, "Số lượng không hợp lệ.");
                return;
            }

            string maNl = _cboNguyenLieu?.SelectedValue?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(maNl) || _nguyenLieuLookup is null)
            {
                return;
            }

            DataRow? nlRow = _nguyenLieuLookup.AsEnumerable().FirstOrDefault(r => string.Equals(Convert.ToString(r["MaNL"]), maNl, StringComparison.OrdinalIgnoreCase));
            if (nlRow is null)
            {
                return;
            }

            DataRow? existing = _dinhMucTable.AsEnumerable().FirstOrDefault(r =>
                string.Equals(Convert.ToString(r["MaDVPV"]), maDvpv, StringComparison.OrdinalIgnoreCase)
                && string.Equals(Convert.ToString(r["MaNL"]), maNl, StringComparison.OrdinalIgnoreCase));

            if (existing is null)
            {
                _dinhMucTable.Rows.Add(maDvpv, maNl, nlRow["TenNL"], soLuong, nlRow["DonViTinh"], nlRow["GiaNhap"], nlRow["SoLuongTon"]);
            }
            else
            {
                existing["SoLuongSuDung"] = soLuong;
            }

            _txtSoLuongDinhMuc.Clear();
            ApplyDinhMucFilterAndGiaVon();
        }

        private void BtnXoaDinhMuc_Click(object? sender, EventArgs e)
        {
            if (_dgvDinhMuc?.CurrentRow is null)
            {
                return;
            }

            string maDvpv = Convert.ToString(_dgvDinhMuc.CurrentRow.Cells["MaDVPV"].Value) ?? string.Empty;
            string maNl = Convert.ToString(_dgvDinhMuc.CurrentRow.Cells["MaNL"].Value) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(maDvpv) || string.IsNullOrWhiteSpace(maNl))
            {
                return;
            }
            string maMon = _currentMaMonDetail;
            if (!string.IsNullOrWhiteSpace(maMon))
            {
                try
                {
                    _quanLiMonAnService.DeleteDinhMuc(maMon, maDvpv, maNl);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa định mức ở DB: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            foreach (DataRow row in _dinhMucTable.Select($"MaDVPV = '{maDvpv.Replace("'", "''")}' AND MaNL = '{maNl.Replace("'", "''")}'"))
            {
                row.Delete();
            }

            _dinhMucTable.AcceptChanges();
            ApplyDinhMucFilterAndGiaVon();
        }

        private static decimal ToDecimalOrZero(object? value)
        {
            if (value is null || value == DBNull.Value)
            {
                return 0m;
            }

            return Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        }

        private bool ForeignKeysExist()
        {
            string maLoai = _cboMaLoai.SelectedValue?.ToString() ?? string.Empty;
            string maDvt = _cboMaDvt.SelectedValue?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(maLoai) || string.IsNullOrWhiteSpace(maDvt))
            {
                return false;
            }

            return _quanLiMonAnService.ForeignKeysExist(maLoai, maDvt);
        }

        private string GenerateNextMaMon()
        {
            try
            {
                return _quanLiMonAnService.GenerateNextMaMon();
            }
            catch
            {
                return "MA1";
            }
        }

        private static string FormatMaMonForDisplay(string? maMonValue)
        {
            if (string.IsNullOrWhiteSpace(maMonValue))
            {
                return string.Empty;
            }

            string value = maMonValue.Trim();
            return value.StartsWith("MA", StringComparison.OrdinalIgnoreCase) ? value.ToUpperInvariant() : $"MA{value}";
        }

        private void btn_QLNCC_Click(object? sender, EventArgs e)
        {
            AdminNavigationManager.Navigate<QuanLiNhaCungCap>(this);
        }

        private void btn_QLKH_Click(object? sender, EventArgs e)
        {
            AdminNavigationManager.Navigate<QuanLiKhachHang>(this);
        }

        private void btn_QLNV_Click(object? sender, EventArgs e)
        {
            AdminNavigationManager.Navigate<QuanLiNhanVien>(this);
        }

        private void btn_QLMA_Click(object? sender, EventArgs e)
        {
        }

        private void btn_QLHDN_Click(object? sender, EventArgs e)
        {
            AdminNavigationManager.Navigate<QuanLiNguyenLieu>(this);
        }

        private void btn_QLHDB_Click(object? sender, EventArgs e)
        {
            AdminNavigationManager.Navigate<LichSuHoaDon>(this);
        }

        private void btn_ThongKe_Click(object? sender, EventArgs e)
        {
            AdminNavigationManager.Navigate<ThongKe>(this);
        }

        private void btn_DangXuat_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Bạn có chắc chắn muốn đăng xuất?",
                "Xác nhận đăng xuất",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                AdminNavigationManager.Logout(this);
            }
        }

        private void btn_DangXuat_MouseEnter(object sender, EventArgs e)
        {
            btn_DangXuat.BackColor = Color.FromArgb(255, 69, 0);
        }

        private void btn_DangXuat_MouseLeave(object sender, EventArgs e)
        {
            btn_DangXuat.BackColor = Color.LightSalmon;
        }

        private void roundedPanel1_Paint(object sender, PaintEventArgs e)
        {
        }

        private void btn_ThongKe_Paint(object sender, PaintEventArgs e)
        {
        }

        private void lblHoTen_Click(object sender, EventArgs e)
        {
        }

        private void lblTrangThai_Click(object sender, EventArgs e)
        {
        }

        private void lblDiaChi_Click(object sender, EventArgs e)
        {
        }

        private void _cboTrangThai_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void pnlSdtInput_Paint(object sender, PaintEventArgs e)
        {
        }

        private void lblNgaySinh_Click(object sender, EventArgs e)
        {
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void roundedPanel4_Paint(object sender, PaintEventArgs e)
        {
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
        }

        private void hcnt_KhungMenuAD_Paint(object sender, PaintEventArgs e)
        {

        }

        private void _cboDvpvChiTiet_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void _txtSoLuongDinhMuc_TextChanged(object sender, EventArgs e)
        {

        }

        private void _cboMaLoai_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
