using BiometricIdentity.Forms;
using BiometricIdentity.Infrastructure.Services;
using BiometricIdentity.Models;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BiometricIdentity.Forms
{
    public partial class UserRegistrationForm : Form
    {
        
        public long Id { get; set; }
        
        public string FirstName { get; set; }
        
        public string LastName { get; set; }
        
        public string UserName { get; set; }
        
        public FingerPrint LeftHand { get; set; }
        
        public FingerPrint RightHand { get; set; }
        public byte[] Picture { get; set; }

        private IRepository<long, User> _userRepository;
        private IRepository<long, AttendanceLog> _attendanceRepository;

        public UserRegistrationForm()
        {
            InitializeComponent();
            InstantiateRepository();
        }

        public UserRegistrationForm(long userId)
        {
            InitializeComponent();
            InstantiateRepository();
            this.Id = userId;
            Task.Run(GetUser);
        }

        private void InstantiateRepository()
        {
            var userRepository = new Repository<long, User>() as IRepository<long, User>;
            var attendanceRepository = new Repository<long, AttendanceLog>() as IRepository<long, AttendanceLog>;
            _userRepository = userRepository;
            _attendanceRepository = attendanceRepository;
        }

        private void UserRegistrationForm_Load(object sender, EventArgs e)
        {

        }

        private void btnUpdateData_Click(object sender, EventArgs e)
        {
            UpdateUser();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            CreateUser();
        }

        private void btnEnrollLeft_Click(object sender, EventArgs e)
        {
            FingerprintForm form = new FingerprintForm();
            if (this.LeftHand != null)
            {
                form = new FingerprintForm(LeftHand);
            }

            if (form.ShowDialog() == DialogResult.OK)
            {
                var fingerprints = form.GetFingerPrints();
                this.LeftHand = fingerprints;
                Task.Run(CheckRegisteredLeftFingers);
            }
        }

        private void btnEnrollRight_Click(object sender, EventArgs e)
        {
            FingerprintForm form = new FingerprintForm();
            if (this.RightHand != null)
            {
                form = new FingerprintForm(RightHand);
            }

            if (form.ShowDialog() == DialogResult.OK)
            {
                var fingerprints = form.GetFingerPrints();
                this.RightHand = fingerprints;
                Task.Run(CheckRegisteredRightFingers);
            }
        }

        private async Task GetUser()
        {
            User user = new User();
            try
            {
                user = await _userRepository.AsQueryable()
                .Include(z => z.RightHand)
                .Include(z => z.LeftHand)
                .FirstOrDefaultAsync(x => x.Id == Id);
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show("An error occured while connecting to the server.");
                return;
            }
            

            if (user != null)
            {
                FirstName = user.FirstName;
                LastName = user.LastName;
                UserName = user.UserName;
                RightHand = user.RightHand;
                LeftHand = user.LeftHand;
                Picture = user.Picture;

                InitializeUIData();
            }
        }

        private void UpdateUser()
        {
            User muser = new User()
            {
                Id = Id,
                FirstName = FirstName,
                LastName = LastName,
                UserName = UserName,
                LeftHand = LeftHand,
                RightHand = RightHand
            };
            User user = null;
            try
            {
                user = _userRepository.AsQueryable()
                .Include(x => x.LeftHand)
                .Include(x => x.RightHand)
                .FirstOrDefault(x => x.Id == Id);
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show("An error occured while connecting to the server.");
                return;
            }
            

            var e = _userRepository.GetValidationErrors(muser);
            if (e.Any())
            {
                MessageBox.Show(string.Join("\n", e.Select(x => x.ErrorMessage.ToArray())));
                return;
            }

            user.FirstName = muser.FirstName;
            user.LastName = muser.LastName;
            user.UserName = muser.UserName;
            user.LeftHand = muser.LeftHand;
            user.RightHand = muser.RightHand;
            user.Picture = Picture;

            _userRepository.Update(user.Id, user);
            var result = _userRepository.SaveChanges();
            if (result.Succeeded)
            {
                MessageBox.Show("User updated successfully!");
            }
            else
            {
                MessageBox.Show($"An error occured while updating user info.\n{result.Message}");
            }
        }

        internal class AttendanceSnapshot
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string UserName { get; set; }
            public string Date { get; set; }
        }

        private void GetAttendance()
        {
            IEnumerable<AttendanceSnapshot> logs = null;
            try
            {
                logs = _attendanceRepository
                .AsQueryable()
                .Include(x => x.User)
                .Where(x => x.UserId == Id)
                .Select(x => new AttendanceSnapshot
                {
                    FirstName = x.User.FirstName,
                    LastName = x.User.LastName,
                    UserName = x.User.UserName,
                    Date = x.RecordDate.ToString()
                });
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show("An error occured while connecting to the server.");
                return;
            }
            

            foreach (var log in logs)
            {
                this.Invoke(new MethodInvoker(delegate(){
                    dataGridAttendance.Rows.Add(log.FirstName, log.LastName, log.UserName, log.Date);
                }));
            }
        }

        private void InitializeUIData()
        {
            this.Invoke(new MethodInvoker(delegate () {
                txtFirstname.Text = FirstName;
                txtLastname.Text = LastName;
                txtUsername.Text = UserName;
                picturePassport.Image = Picture != null? Image.FromStream(new MemoryStream(Picture)): null;
            }));
                
            Task.Run(CheckRegisteredLeftFingers);
            Task.Run(CheckRegisteredRightFingers);
            GetAttendance();
        }

        private void CreateUser()
        {
            User user = new User()
            {
                FirstName = FirstName,
                LastName = LastName,
                UserName = UserName,
                LeftHand = LeftHand,
                RightHand = RightHand,
                Picture = Picture
            };

            var e = _userRepository.GetValidationErrors(user);
            if (e.Any())
            {
                MessageBox.Show(string.Join("\n", e.Select(x => x.ErrorMessage).ToArray()));
                return;
            }

            _userRepository.Insert(user);
            var result = _userRepository.SaveChanges();
            if (result.Succeeded)
            {
                MessageBox.Show("User created successfully!");
            }
            else
            {
                MessageBox.Show($"An error occured while creating the user\n{result.Message}");
            }
        }

        private void CheckRegisteredLeftFingers()
        {
            if (LeftHand != null)
            {
                this.Invoke(new MethodInvoker(delegate () {

                    checkBoxLT.Checked = LeftHand.Thumb != null;
                    checkBoxLI.Checked = LeftHand.IndexFinger != null;
                    checkBoxLM.Checked = LeftHand.MiddleFinger != null;
                    checkBoxLR.Checked = LeftHand.RingFinger != null;
                    checkBoxLL.Checked = LeftHand.LittleFinger != null;
                }));
            }
        }

        private void CheckRegisteredRightFingers()
        {
            if (RightHand != null)
            {
                this.Invoke(new MethodInvoker(delegate () {

                    checkBoxRT.Checked = RightHand.Thumb != null;
                    checkBoxRI.Checked = RightHand.IndexFinger != null;
                    checkBoxRM.Checked = RightHand.MiddleFinger != null;
                    checkBoxRR.Checked = RightHand.RingFinger != null;
                    checkBoxRL.Checked = RightHand.LittleFinger != null;
                }));
            }
        }

        private void txtUsername_TextChanged(object sender, EventArgs e)
        {
            this.UserName = txtUsername.Text;
        }

        private void txtFirstname_TextChanged(object sender, EventArgs e)
        {
            this.FirstName = txtFirstname.Text;
        }

        private void txtLastname_TextChanged(object sender, EventArgs e)
        {
            this.LastName = txtLastname.Text;
        }

        private void picturePassport_DoubleClick(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Multiselect = false;
            openFile.Title = "Select Image";
            openFile.Filter = "JPG|*.jpg|PNG|*.png";
            openFile.DefaultExt = "jpg";
            openFile.AddExtension = true;
            openFile.InitialDirectory = SpecialDirectories.MyPictures;
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                this.picturePassport.Image = Image.FromFile(openFile.FileName);
                var ms = new MemoryStream();
                this.picturePassport.Image.Save(ms, ImageFormat.Jpeg);
                Picture = ms.ToArray();
            }
        }
    }
}
