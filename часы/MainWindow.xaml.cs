using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Speech.Synthesis;
using System.Media;
using Microsoft.VisualBasic.Devices;
using System.Threading;


namespace Chas
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TimeZoneInfo curZone;

        // Неизменяемая система исчисления.
        const double NumberSystem = 60;
        private bool vikl = false;
        // Количество градусов на единицу системы исчисления.
        // Базовый угол для минут и секунд.
        const double baseAngleNumberSystem = 360 / NumberSystem;

        // Базовый угол для часов.
        const double baseAngleHour = 30;



        public MainWindow()
        {
            DispatcherTimer timer1 = new DispatcherTimer();
            timer1.Interval = TimeSpan.FromMilliseconds(1000);
            timer1.Tick += timer1_Tick;
            timer1.Start();
            InitializeComponent();
            timer1_Tick(null, null);
            

            StartClock();

            curZone = TimeZoneInfo.Local;
            spisok.ItemsSource = TimeZoneInfo.GetSystemTimeZones();
            spisok.SelectedItem = curZone;

            test.Visibility = Visibility.Hidden;
            Forma.Visibility = Visibility.Visible;



        }



        #region Вычисление текущего времени

        private void Timer_Tick(object sender, EventArgs e)
        {
            var rotateSecondArrow = new RotateTransform();
            var rotateMinuteArrow = new RotateTransform();
            var rotateHourArrow = new RotateTransform();


            // Данные текущего времени.
            DateTime dt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, curZone);
            int sec = dt.Second;
            int min = dt.Minute;
            int hour = dt.Hour;



            // Вычисленный угол для секундной стрелки.
            rotateSecondArrow.Angle = baseAngleNumberSystem * sec;

            // Вращение стрелки на вычисленный угол.
            SecondArrow.RenderTransform = rotateSecondArrow;



            // Угол минутной стрелки от количества полных минут плюс
            // угол секунд приведенный к долям текущей минуты.
            rotateMinuteArrow.Angle = (min * baseAngleNumberSystem) + (rotateSecondArrow.Angle / 60.0);

            MinuteArrow.RenderTransform = rotateMinuteArrow;



            // Данные часа конвертируем в 12-часовой вид,
            // вычисляем угол полных часов плюс
            // угол минут приведенный к долям текущего часа.
            rotateHourArrow.Angle = (hour - 12) * baseAngleHour + rotateMinuteArrow.Angle / 12;

            HourArrow.RenderTransform = rotateHourArrow;



            // После вычисления всех углов и поворотов стрелок,
            // включаем видимость формы.
            this.Show();
        }


        #endregion



        #region Система перемещения окна

        bool move = false;
        Point constPosition;



        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

            // Зафиксируем неизменяемую позицию.
            constPosition = e.GetPosition(this);

            // Разрешаем движение.
            move = true;

            // Чтобы мышь не теряла окно, даже если окно скрывается под тормост окнами.
            this.CaptureMouse();

        }



        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {

            if (move == true)
            {
                // Вычисляем разницу между бывшим и текущим положением курсора от края окна.
                double deltaX = e.GetPosition(this).X - constPosition.X;
                double deltaY = e.GetPosition(this).Y - constPosition.Y;


                // Положение окна тут же корректируем на вычисленную величину(разницу).
                this.Left += deltaX;
                this.Top += deltaY;
            }

        }



        private void Window_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

            // Запрещаем движение и отпускаем мышь.
            move = false;
            this.ReleaseMouseCapture();

        }

        #endregion



        #region Дополнительные методы

        // Запуск часов.
        void StartClock()
        {
            // Скрываем форму пока не заработали часы.
            // После первого события Timer_Tick,
            // когда стрелки скорректируют своё положение,
            // сделаем форму видимой.
            this.Hide();

            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += Timer_Tick;
            timer.Start();
        }





        // Минутная маркировка циферблата
        void MinuteMarks()
        {

            for (int i = 0; i < 60; i++)
            {
                // Контейнер для метки.
                var b = new Border()
                {
                    RenderTransformOrigin = new Point(0.5, 0.5),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center,

                    // Базовая ориентация контейнера горизонтальноеЮ
                    // поэтому высота играет роль толщины.
                    Height = 2
                };


                // Непосредственно метка.
                var b1 = new Border()
                {
                    Background = Brushes.Brown,
                    HorizontalAlignment = HorizontalAlignment.Right,

                    // Базовая ориентация метки горизонтальная,
                    // поэтому ширина визуально является длиной метки.
                    Width = 10
                };


                b.Child = b1;


                // Вращаем только сам контейнер.
                var rotate = new RotateTransform(i * 6);
                b.RenderTransform = rotate;


                // Исключаем из маркировки метки 0 и 
                // через каждые 30 градусов. 
                // В этом месте будут часовые метки.
                if (i * 6 % 30 != 0)
                {
                    ClockFace.Children.Add(b);
                }
            }

        }



        // Часовая маркировка циферблата
        void HourMarks()
        {

            for (int i = 0; i < 12; i++)
            {
                var b = new Border()
                {
                    // Определяет толщину метки.
                    Height = 12,

                    RenderTransformOrigin = new Point(0.5, 0.5),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center
                };


                var b1 = new Border()
                {
                    // Определяет длину метки.
                    Width = 8,

                    Background = Brushes.Black,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    BorderBrush = Brushes.Black
                };


                b.Child = b1;

                // Часовые метки через 30 градусов.
                // Вращаем только контейнер.
                var rotate = new RotateTransform(i * 30);
                b.RenderTransform = rotate;


                ClockFace.Children.Add(b);
            }

        }



        #endregion



        #region Закрытие окна

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion



        #region Завод изготовления часов

        // 
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,

                // Значение по умолчанию — true в .NET Framework приложениях и 
                // false в приложениях .NET Core.
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);
        }



        #endregion

        private void spisok_Selected(object sender, RoutedEventArgs e)
        {
            if (spisok.SelectedItem == null) return;
            curZone = (TimeZoneInfo)spisok.SelectedItem;
        }

        void timer1_Tick(object sender, EventArgs e)
        {
            if (curZone == null) return;
            DateTime dt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, curZone);
            var h = dt.Hour;
            var m = dt.Minute;
            var s = dt.Second;
            test.Text = dt.ToString("HH:mm:ss");


            if (!vikl && s % 30 == 0)
            {
                new Thread(() =>
                {
                    SoundPlayer sp = new SoundPlayer();
                    if (h > 20)
                    {
                        PlaySound(sp, "20");
                        PlaySound(sp, (h - 20).ToString());
                    }
                    else
                    {
                        PlaySound(sp, h.ToString());
                    }

                    if (m == 0) PlaySound(sp, "0");
                    else if (m < 3) PlaySound(sp, m + "a");
                    else if (m < 21) PlaySound(sp, m.ToString());
                    else
                    {
                        int d = ((int)(m / 10) * 10);
                        PlaySound(sp, d.ToString());
                        m -= d;
                        if (m == 0) PlaySound(sp, "0");
                        else if (m < 3) PlaySound(sp, m + "a");
                        else PlaySound(sp, m.ToString());
                    }
                }).Start();
            }
        }

        void PlaySound(SoundPlayer sp, String name)
        {
            if (name.Equals("0")) return;
            sp.SoundLocation = $"C:/Users/Comp0720/Desktop/часы1/часы/NewFolder1/{name}.wav";
            sp.Play();
            Thread.Sleep(1300);
        }
       

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            test.Visibility = Visibility.Hidden;
            Forma.Visibility = Visibility.Visible;
        }
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            test.Visibility = Visibility.Visible;
            Forma.Visibility = Visibility.Hidden;
        }
        private void button3_Click(object sender, RoutedEventArgs e)
        {
            vikl = !vikl;
        }
    }
    
}
