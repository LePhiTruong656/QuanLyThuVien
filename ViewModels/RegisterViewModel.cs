using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace LibraryManagementFE.ViewModels
{
    public class RegisterViewModel : INotifyPropertyChanged
    {
        // ── Step navigation ─────────────────────────────────────────────
        private int _currentStep = 1;
        public int CurrentStep
        {
            get => _currentStep;
            set
            {
                _currentStep = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CardTitle));
                OnPropertyChanged(nameof(StepLabel));
                OnPropertyChanged(nameof(ContinueLabel));
                OnPropertyChanged(nameof(ShowBackButton));
                OnPropertyChanged(nameof(Step2Opacity));
                OnPropertyChanged(nameof(Step3Opacity));
            }
        }

        public string CardTitle => CurrentStep switch
        {
            1 => "Tạo tài khoản",
            2 => "Thông tin cá nhân",
            _ => "Xác nhận đăng ký"
        };

        public string StepLabel => CurrentStep switch
        {
            1 => "Bước 1 / 3  ·  Tài khoản",
            2 => "Bước 2 / 3  ·  Thông tin",
            _ => "Bước 3 / 3  ·  Xác nhận"
        };

        public string ContinueLabel => CurrentStep == 3 ? "Hoàn tất đăng ký" : "Tiếp tục →";
        public bool ShowBackButton => CurrentStep > 1;
        public double Step2Opacity => CurrentStep >= 2 ? 1.0 : 0.25;
        public double Step3Opacity => CurrentStep >= 3 ? 1.0 : 0.25;

        // ── Step 1: Account ─────────────────────────────────────────────
        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowEmailPlaceholder));
                if (!string.IsNullOrEmpty(_emailError)) ValidateEmail();
            }
        }
        public bool ShowEmailPlaceholder => string.IsNullOrEmpty(_email);

        private string _emailError = string.Empty;
        public string EmailError
        {
            get => _emailError;
            private set { _emailError = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasEmailError)); }
        }
        public bool HasEmailError => !string.IsNullOrEmpty(_emailError);

        private string _passwordError = string.Empty;
        public string PasswordError
        {
            get => _passwordError;
            private set { _passwordError = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasPasswordError)); }
        }
        public bool HasPasswordError => !string.IsNullOrEmpty(_passwordError);

        private string _confirmPasswordError = string.Empty;
        public string ConfirmPasswordError
        {
            get => _confirmPasswordError;
            private set { _confirmPasswordError = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasConfirmPasswordError)); }
        }
        public bool HasConfirmPasswordError => !string.IsNullOrEmpty(_confirmPasswordError);

        private void ValidateEmail()
        {
            if (string.IsNullOrWhiteSpace(_email))
                EmailError = "Email không được để trống.";
            else if (!Regex.IsMatch(_email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                EmailError = "Email không hợp lệ.";
            else
                EmailError = string.Empty;
        }

        public bool ValidateStep1(string password, string confirmPassword)
        {
            ValidateEmail();

            if (string.IsNullOrWhiteSpace(password))
                PasswordError = "Mật khẩu không được để trống.";
            else if (password.Length < 8)
                PasswordError = "Mật khẩu tối thiểu 8 ký tự.";
            else
                PasswordError = string.Empty;

            if (string.IsNullOrWhiteSpace(confirmPassword))
                ConfirmPasswordError = "Vui lòng xác nhận mật khẩu.";
            else if (password != confirmPassword)
                ConfirmPasswordError = "Mật khẩu xác nhận không khớp.";
            else
                ConfirmPasswordError = string.Empty;

            return !HasEmailError && !HasPasswordError && !HasConfirmPasswordError;
        }

        // ── Step 2: Personal info ────────────────────────────────────────
        private string _fullName = string.Empty;
        public string FullName
        {
            get => _fullName;
            set
            {
                _fullName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowFullNamePlaceholder));
                if (!string.IsNullOrEmpty(_fullNameError)) ValidateFullName();
            }
        }
        public bool ShowFullNamePlaceholder => string.IsNullOrEmpty(_fullName);

        private string _studentId = string.Empty;
        public string StudentId
        {
            get => _studentId;
            set
            {
                _studentId = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowStudentIdPlaceholder));
                if (!string.IsNullOrEmpty(_studentIdError)) ValidateStudentId();
            }
        }
        public bool ShowStudentIdPlaceholder => string.IsNullOrEmpty(_studentId);

        private string _cardType = "Sinh viên";
        public string CardType
        {
            get => _cardType;
            set
            {
                _cardType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsStudent));
                OnPropertyChanged(nameof(IsLecturer));
                OnPropertyChanged(nameof(StudentIdLabel));
                OnPropertyChanged(nameof(StudentIdPlaceholder));
            }
        }

        public bool IsStudent
        {
            get => _cardType == "Sinh viên";
            set { if (value) CardType = "Sinh viên"; }
        }

        public bool IsLecturer
        {
            get => _cardType == "Giảng viên";
            set { if (value) CardType = "Giảng viên"; }
        }

        public string StudentIdLabel => IsStudent ? "MSSV" : "Mã giảng viên";
        public string StudentIdPlaceholder => IsStudent ? "22520001" : "GV001";

        private string _dateOfBirth = string.Empty;
        public string DateOfBirth
        {
            get => _dateOfBirth;
            set
            {
                _dateOfBirth = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowDateOfBirthPlaceholder));
            }
        }
        public bool ShowDateOfBirthPlaceholder => string.IsNullOrEmpty(_dateOfBirth);

        private string _phone = string.Empty;
        public string Phone
        {
            get => _phone;
            set
            {
                _phone = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowPhonePlaceholder));
            }
        }
        public bool ShowPhonePlaceholder => string.IsNullOrEmpty(_phone);

        // Step 2 errors
        private string _fullNameError = string.Empty;
        public string FullNameError
        {
            get => _fullNameError;
            private set { _fullNameError = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasFullNameError)); }
        }
        public bool HasFullNameError => !string.IsNullOrEmpty(_fullNameError);

        private string _studentIdError = string.Empty;
        public string StudentIdError
        {
            get => _studentIdError;
            private set { _studentIdError = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasStudentIdError)); }
        }
        public bool HasStudentIdError => !string.IsNullOrEmpty(_studentIdError);

        private string _dateOfBirthError = string.Empty;
        public string DateOfBirthError
        {
            get => _dateOfBirthError;
            private set { _dateOfBirthError = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasDateOfBirthError)); }
        }
        public bool HasDateOfBirthError => !string.IsNullOrEmpty(_dateOfBirthError);

        private void ValidateFullName()
        {
            FullNameError = string.IsNullOrWhiteSpace(_fullName)
                ? "Họ và tên không được để trống."
                : string.Empty;
        }

        private void ValidateStudentId()
        {
            StudentIdError = string.IsNullOrWhiteSpace(_studentId)
                ? $"{StudentIdLabel} không được để trống."
                : string.Empty;
        }

        public bool ValidateStep2()
        {
            ValidateFullName();
            ValidateStudentId();

            if (!string.IsNullOrWhiteSpace(_dateOfBirth) &&
                !DateTime.TryParseExact(_dateOfBirth, "dd/MM/yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out _))
                DateOfBirthError = "Định dạng ngày sinh: DD/MM/YYYY.";
            else
                DateOfBirthError = string.Empty;

            return !HasFullNameError && !HasStudentIdError && !HasDateOfBirthError;
        }

        // ── Step 3: Confirm ──────────────────────────────────────────────
        private bool _agreeToTerms;
        public bool AgreeToTerms
        {
            get => _agreeToTerms;
            set { _agreeToTerms = value; OnPropertyChanged(); if (_showTermsError) ShowTermsError = false; }
        }

        private bool _showTermsError;
        public bool ShowTermsError
        {
            get => _showTermsError;
            private set { _showTermsError = value; OnPropertyChanged(); }
        }

        public bool ValidateStep3()
        {
            ShowTermsError = !_agreeToTerms;
            return _agreeToTerms;
        }

        public string DisplayPhone => string.IsNullOrWhiteSpace(_phone) ? "—" : _phone;
        public string DisplayDateOfBirth => string.IsNullOrWhiteSpace(_dateOfBirth) ? "—" : _dateOfBirth;

        // ─────────────────────────────────────────────────────────────────
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
