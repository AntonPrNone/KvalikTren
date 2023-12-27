using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace KvalikTren.View
{
    /// <summary>
    /// Логика взаимодействия для LoginView.xaml
    /// </summary>
    public partial class LoginView : Window
    {

        private string LoginText;
        private string PasswordText;
        private bool _isAnimationRunning = false;

        public LoginView()
        {
            InitializeComponent();
        }


        private void Log_Button_Click(object sender, RoutedEventArgs e)
        {
            LoginText = Login_TextBox.Text;
            PasswordText = Password_TextBox.Password;

            var result = DB_Interaction.CheckLogin(LoginText, PasswordText);
            if (result.success)
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }

            else
            {
                ShowErrorMessage(result.errorMessage);
            }
        }

        private void ShowErrorMessage(string errorMessage)
        {
            Mess_TextBlock.Text = errorMessage;
            AnimateBorder();
        }

        private void AnimateBorder()
        {
            if (_isAnimationRunning) return;

            _isAnimationRunning = true;

            // Очищаем все анимации перед добавлением новых
            BorderMess_Border.BeginAnimation(UIElement.OpacityProperty, null);
            BorderMess_Border.BeginAnimation(TranslateTransform.XProperty, null);

            BorderMess_Border.Opacity = 1;

            TranslateTransform translateTransform = new TranslateTransform();
            BorderMess_Border.RenderTransform = translateTransform;

            DoubleAnimation moveAnimation = new DoubleAnimation
            {
                From = ActualWidth + 20,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.75)
            };

            DoubleAnimation opacityAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(1)
            };

            moveAnimation.Completed += async (sender, e) =>
            {
                await Task.Delay(5000);

                opacityAnimation.Completed += (opacitySender, opacityE) =>
                {
                    _isAnimationRunning = false;
                };

                BorderMess_Border.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
            };

            translateTransform.BeginAnimation(TranslateTransform.XProperty, moveAnimation);
        }

        // ----------------------------------------------------------------------

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
