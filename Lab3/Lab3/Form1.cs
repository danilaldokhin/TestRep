using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace Lab3
{

    public partial class Form1 : Form
    {
        public int radius_min;
        public int radius_max;
        public int density;
        public int point_eq;
        public Bitmap bmp_filter;
        public Bitmap bmp_binarization;
        public Bitmap bmp3;
        public Bitmap bmp_start;
        public Bitmap bmp_bin_start_pic;
        public Bitmap bmp_object;
        public Bitmap bmp_green;
        public Bitmap bmp_red;
        public Bitmap bmp_blue;
        public Bitmap bmp_sample;

        public Form1()
        {
            InitializeComponent();
            string[] files = System.IO.Directory.GetFiles(@"C:\C#\images", "*.png");
           
            for (int i = 0; i < files.GetLength(0); i++) 
            { 
                comboBox1.Items.Add(files[i]);
            }
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            button6.Enabled = false;
            button5.Enabled = false;
            files = System.IO.Directory.GetFiles(@"C:\C#\Signs", "*.gif"); 
           
            for (int i = 0; i < files.GetLength(0); i++)                                            
            {                                                                                       
                comboBox2.Items.Add(files[i]);
            }
        }

        class graph_key
        {
            public static int Max_red;
            public static int Min_red;
            public static int Max_green;
            public static int Min_green;
            public static int Max_blue;
            public static int Min_blue;
        }

        public struct clast_center
        {
            public int x_center;
            public int y_center;
        }

        public struct object_border
        {
            public int x_left;
            public int y_up;
        }

        public static Bitmap Resize_bmp(Bitmap bmp, double scale_width, double scale_height)        //функция мастабирования битмапа
        {
            var w = (int)(bmp.Width * scale_width);
            var h = (int)(bmp.Height * scale_height);
            var res = new Bitmap(w, h);
            using (var gr = Graphics.FromImage(res))
            {
                gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;       //используем бикубическую интерполяцию
                gr.DrawImage(bmp, 0, 0, w, h);
            }
            return res;
        }

        struct found_object
        {
            public int coord_x;
            public int coord_y;
            public int rect_width;
            public int rect_height;
        }

        found_object[] Found_Objects = new found_object[50];       //инициализация массива найденных объектов

        public static int check_density(int density)                //функция проверки плотности
        {
            string msg_1 = "Введите плотность от 1 до 100";
            if (density <= 0 || density > 100)
            {
                if (density <= 0)
                {
                    density = 1;
                    MessageBox.Show(msg_1);
                }
                else
                {
                    if (density > 100)
                    {
                        density = 100;
                        MessageBox.Show(msg_1);
                    }
                }
            }
            return density;
        }

        public static int min_radius_check(int radius_min, int pic_border)          //функция проверки минимального радиуса
        {
            string msg_min = "Введите корректный минимальный радиус";
            if (radius_min <= 0 || radius_min > pic_border)
            {
                if (radius_min <= 0)
                {
                    radius_min = 0;
                    MessageBox.Show(msg_min);
                }
                else
                {
                    if (radius_min > pic_border)
                    {
                        radius_min = pic_border - 1;
                        MessageBox.Show(msg_min);
                    }
                }
            }
            return radius_min;
        }

        public static int max_radius_check(int radius_max, int pic_border)       //функция проверки максимального радиуса
        {
            string msg = "Введите корректный минимальный радиус";
            if (radius_max <= 0 || radius_max >= pic_border)
            {
                if (radius_max <= 0)
                {
                    MessageBox.Show(msg);
                    radius_max = 1;
                }
                else
                {
                    if (radius_max > pic_border)
                    {
                        MessageBox.Show(msg);
                        radius_max = pic_border;
                    }
                }
            }
            return radius_max;
        }

        public static int min_max_compare(int radius_min, int radius_max)                  //функция сравнения минимального и максимаьного значения
        {
            if (radius_max <= radius_min)
            {
                MessageBox.Show("Минимальный радиус больше максимального");
                radius_min = radius_max - 1;
            }
            return radius_min;
        }

        public static bool coord_correct_check(int start_coord_x, int start_coord_y, int radius_min)   // функция проверки начальных координат
        {                                                                                              // нач коорд больше радиуса
            bool corret = true;
            if (start_coord_x < radius_min || start_coord_y < radius_min)
            {
                corret = false;
            }
            return corret;
        }

        public static clast_center clast_center_find(int coord_x, int coord_y, int finish_x, int finish_y, Bitmap bmp_binar)  //функция поиска центра кластера
        {
            int m; //весовой коэффициент пикселя
            int count_x = 0;
            int count_y = 0;
            int coord_sum_x = 0;
            int coord_sum_y = 0;
            clast_center coordinates;
            coordinates.x_center = 0;
            coordinates.y_center = 0;
            for (int i = coord_x; i < finish_x; i++)
            {
                for (int j = coord_y; j < finish_y; j++)
                {
                    if (i < bmp_binar.Width && j < bmp_binar.Height) //проверка условия на выход за границы битмапа, если бы его не было, вылетала бы ошибка
                    {
                        Color px = bmp_binar.GetPixel(i, j);
                        if (px.R == 0 || px.G == 0 || px.B == 0) //если пиксель черный-весовой коэффициент = 0
                        {
                            m = 0;
                        }
                        else
                        {
                            m = 1;
                        }
                        coord_sum_x = coord_sum_x + m * i;
                        coord_sum_y = coord_sum_y + m * j;
                        count_x = count_x + m;
                        count_y = count_y + m;
                    }
                }
            }
            coordinates.x_center = coord_sum_x / count_x;
            coordinates.y_center = coord_sum_y / count_y;
            return (coordinates);
        }

        public static object_border find_border(int coord_x, int coord_y, int finish_x, int finish_y, Bitmap bmp_binar)   //функция поиска границы слева и сверху
        {
            object_border coordinates;
            coordinates.x_left = finish_x;
            coordinates.y_up = finish_y;
            for (int i = coord_x; i < finish_x; i++) //ищем крайний левый писксель, по нему проводим границу
            {
                for (int j = coord_y; j < finish_y; j++)
                {
                    if (i < bmp_binar.Width && j < bmp_binar.Height)
                    {
                        Color px = bmp_binar.GetPixel(i, j);

                        if (px.R != 0 || px.G != 0 || px.B != 0)
                        {
                            if (coordinates.y_up >= j)
                            {
                                coordinates.y_up = j;
                            }
                        }
                    }
                }
            }
            for (int j = coord_y; j < finish_y; j++) //ищем крайний верхний пиксель и проводим границу
            {
                for (int i = coord_x; i < finish_x; i++)
                {
                    if (i < bmp_binar.Width && j < bmp_binar.Height)
                    {
                        Color px = bmp_binar.GetPixel(i, j);

                        if (px.R != 0 || px.G != 0 || px.B != 0)
                        {
                            if (coordinates.x_left >= i)
                            {
                                coordinates.x_left = i;
                            }
                        }
                    }
                }
            }
            return coordinates;
        }

        public static Bitmap binarization(Bitmap bmp_binarize, int width, int height)
        {
            for (int i = 0; i < width; i++) //бинаризация
            {
                for (int j = 0; j < height; j++)
                {
                    Color fin = Color.FromArgb(0, 0, 0);
                    Color pix = bmp_binarize.GetPixel(i, j);
                    if (pix.R >= graph_key.Max_red || pix.R <= graph_key.Min_red)
                    {
                        bmp_binarize.SetPixel(i, j, fin);
                    }
                    else { bmp_binarize.SetPixel(i, j, Color.White); }
                    if (pix.B >= graph_key.Max_blue || pix.B <= graph_key.Min_blue)
                    {
                        bmp_binarize.SetPixel(i, j, fin);
                    }
                    else { bmp_binarize.SetPixel(i, j, Color.White); }

                    if (pix.G >= graph_key.Max_green || pix.G <= graph_key.Min_green)
                    {
                        bmp_binarize.SetPixel(i, j, fin);
                    }
                    else { bmp_binarize.SetPixel(i, j, Color.White); }
                }
            }
            return bmp_binarize;
        }

        private void button2_Click(object sender, EventArgs e) //применение граф фильтра
        {
            if (comboBox1.Text != "Список файлов в папке" && comboBox1.Text != "")
            {
                button3.Enabled = true;
                button4.Enabled = true;
                bmp_filter = (Bitmap)Image.FromFile(comboBox1.Text); //Фильтация картинки
                graph_key.Max_red = Convert.ToInt32(numericUpDownRedMax.Value);
                graph_key.Max_green = Convert.ToInt32(numericUpDownGreenMax.Value);
                graph_key.Max_blue = Convert.ToInt32(numericUpDownBlueMax.Value);
                graph_key.Min_red = Convert.ToInt32(numericUpDownRedMin.Value);
                graph_key.Min_green = Convert.ToInt32(numericUpDownGreenMin.Value);
                graph_key.Min_blue = Convert.ToInt32(numericUpDownBlueMin.Value);
                Color max_col = Color.FromArgb(graph_key.Max_red, graph_key.Max_green, graph_key.Max_blue);
                Color min_col = Color.FromArgb(graph_key.Min_red, graph_key.Min_green, graph_key.Min_blue);
                for (int i = 0; i < pictureBox1.Width; i++)
                {
                    for (int j = 0; j < pictureBox1.Height; j++)
                    {
                        Color fin = Color.FromArgb(0, 0, 0);
                        Color pix = bmp_filter.GetPixel(i, j);
                        if (pix.R >= graph_key.Max_red || pix.R <= graph_key.Min_red)
                        {
                            bmp_filter.SetPixel(i, j, fin);
                        }
                        if (pix.B >= graph_key.Max_blue || pix.B <= graph_key.Min_blue)
                        {
                            bmp_filter.SetPixel(i, j, fin);
                        }
                        if (pix.G >= graph_key.Max_green || pix.G <= graph_key.Min_green)
                        {
                            bmp_filter.SetPixel(i, j, fin);
                        }
                    }
                }
                pictureBox1.Image = bmp_filter;
            }
            else
            {
                MessageBox.Show("Выбран файл загрузки");
            }
        }

        private void button3_Click(object sender, EventArgs e) //бинаризация
        {
            pictureBox1.Image = Image.FromFile(comboBox1.Text);
            bmp_binarization = bmp_filter;
            bmp_binarization = binarization(bmp_binarization, pictureBox1.Width, pictureBox1.Height); //вызов функции бинаризации
            pictureBox2.Image = bmp_binarization;
            bmp_bin_start_pic = (Bitmap)pictureBox2.Image;
        }

        private void numericUpDownRedMax_ValueChanged(object sender, EventArgs e)
        {
            graph_key.Max_red = Convert.ToInt32(numericUpDownRedMax.Value); //  для все NumericUpDown просто изменение 
        } // пороговых значений фильтра

        private void numericUpDownRedMin_ValueChanged(object sender, EventArgs e)
        {
            graph_key.Max_red = Convert.ToInt32(numericUpDownRedMin.Value);
        }

        private void numericUpDownGreenMax_ValueChanged(object sender, EventArgs e)
        {
            graph_key.Max_green = Convert.ToInt32(numericUpDownGreenMax.Value);
        }

        private void numericUpDownGreenMin_ValueChanged(object sender, EventArgs e)
        {
            graph_key.Min_green = Convert.ToInt32(numericUpDownGreenMin.Value);

        }

        private void numericUpDownBlueMax_ValueChanged(object sender, EventArgs e)
        {
            graph_key.Max_blue = Convert.ToInt32(numericUpDownBlueMax.Value);
        }

        private void numericUpDownBlueMin_ValueChanged(object sender, EventArgs e)
        {
            graph_key.Min_blue = Convert.ToInt32(numericUpDownBlueMin.Value);
        }

        private void button4_Click(object sender, EventArgs e) //алгоритм поиска
        {
            listBox1.Items.Clear();
            int count_of_objects = 0;
            bool obj_found = false;
            int x = 0; int y = 0;
            int eq_of_points = 0; //количество точек
            Bitmap bmp_clast = new Bitmap(pictureBox2.Height, pictureBox2.Width);
            bmp_clast = (Bitmap)pictureBox2.Image;
            Graphics g = pictureBox2.CreateGraphics();
            //int finish_cpunt = 0;
            int pic_border = pictureBox1.Height / 2; // макс радиус - половина высоты картинки
            try
            {
                radius_max = Int32.Parse(textBox4.Text);
                radius_min = Int32.Parse(textBox3.Text);
                density = Int32.Parse(textBox2.Text);
            }
            catch (SystemException)
            {
                string msg = "Введите корректные значения";
                MessageBox.Show(msg);
            }
            density = check_density(density); // проверки функциями выше
            radius_min = min_radius_check(radius_min, pic_border);
            radius_max = max_radius_check(radius_max, pic_border);
            radius_min = min_max_compare(radius_min, radius_max);
            int start_i = radius_min; //смещение стартовых координат на минимальный радиус
            int start_j = radius_min;
            textBox2.Text = Convert.ToString(density);
            textBox3.Text = Convert.ToString(radius_min);
            textBox4.Text = Convert.ToString(radius_max);
            Bitmap bmp_save = new Bitmap(pictureBox2.Width, pictureBox2.Height);
            int area = (bmp_clast.Height / 2 - 2 * radius_min - 1) * (bmp_clast.Width / 2 - 2 * radius_min - 1); //поиск площади, не уверен
            int count_of_iteration = 0;
            Again:
            while (obj_found == false && count_of_iteration < area) // Пока объект не найден лии все пиксели не перебраны
            {
                for (int i = start_i; i < bmp_clast.Width - radius_min; i = i + 3)
                {
                    for (int j = start_j; j < bmp_clast.Height - radius_min; j = j + 3)
                    {
                        pictureBox2.Image = bmp_binarization;
                        Rectangle rect2 = new Rectangle(i - radius_min, j - radius_min, 2 * radius_min + 1, 2 * radius_min + 1);
                        // g.DrawRectangle(new Pen(Color.Black), rect2);
                        int y_border = bmp_clast.Height - 2;
                        int x_border = bmp_clast.Width - 2;
                        int start_x = i - radius_min; //Задание сдвига на радиус
                        int start_y = j - radius_min;
                        int finish_x = i + radius_min - 3;
                        int finish_y = j + radius_min - 3;
                        if (finish_x < x_border && finish_y < y_border) // обработка пикселей в заданном квадрате
                        {
                            eq_of_points = 0;
                            for (x = start_x; x < finish_x; x++)
                            {
                                for (y = start_y; y < finish_y; y++)
                                {
                                    Color px = bmp_clast.GetPixel(x, y);
                                    //bmp_save.SetPixel(x, y, px);
                                    if (px.R == 255 && px.G == 255 && px.B == 255)
                                    {
                                        eq_of_points++; //Подсчет белых пикселей
                                    }
                                }
                            }
                            count_of_iteration++;
                            // визуализация поиска, чтобы посмотреть логику работы, можно расскомментить и показать
                            Color color1 = Color.FromArgb(50, 255, 255, 0);
                            Rectangle rect1 = new Rectangle(i - radius_min, j - radius_min, 2 * radius_min + 1, 2 * radius_min + 1);
                            g.DrawRectangle(new Pen(color1), rect1);
                            int square = ((2 * radius_min + 1) * (2 * radius_min + 1)); //ПОиск площади квадрата
                            double final_density;
                            final_density = (double)eq_of_points / square; //расчет финальной плотности
                            final_density = (double)final_density * 100;
                            label10.Text = Convert.ToString(final_density);
                            if (density < final_density) // если расчетная плотность больше заданной
                            {  //увеличиваем радиус поиска
                                MessageBox.Show("Увеличение радиуса");
                                start_i = start_i + 10;
                                start_j = start_j + 10;
                                radius_min = radius_min + 10;
                            }
                            if (radius_min >= radius_max)  //Если радиус поиска превышает 25
                            { // объект найден
                                MessageBox.Show("Объект найден");
                                clast_center coordinates_center;
                                object_border coordinates_border;
                                coordinates_center = clast_center_find(i - radius_min, j - radius_min, i + 2 * radius_min + 1, j + 2 * radius_min + 1, bmp_binarization);   //поиск центра масс кластера
                                coordinates_border = find_border(i - radius_min, j - radius_min, i + 2 * radius_min + 1, j + 2 * radius_min + 1, bmp_binarization);         // и границ с помощью функций
                                int x_width = coordinates_center.x_center - coordinates_border.x_left; //поиск расстояния от центра до границы
                                int y_height = coordinates_center.y_center - coordinates_border.y_up;
                                Color color = Color.Red; //Отрисовывем границу
                                Rectangle rect = new Rectangle(i - radius_min + 10, j - radius_min + 10, 2 * radius_min + 1, 2 * radius_min + 1);
                                //g.DrawRectangle(new Pen(color), rect);
                                obj_found = true;
                                Rectangle rect_center_border = new Rectangle(coordinates_border.x_left, coordinates_border.y_up, x_width * 2 + 1, y_height * 2 + 1);
                                Rectangle rect_cent = new Rectangle(coordinates_center.x_center - 1, coordinates_center.y_center - 1, 3, 3);
                                g.DrawRectangle(new Pen(color), rect_cent);
                                g.DrawRectangle(new Pen(Color.GreenYellow), rect_center_border); //отрисовка границы объекта
                                start_i = i + radius_min;
                                Found_Objects[count_of_objects].coord_x = rect_center_border.X; //запись параметрво объекта в массив
                                Found_Objects[count_of_objects].coord_y = rect_center_border.Y;
                                Found_Objects[count_of_objects].rect_height = rect_center_border.Height;
                                Found_Objects[count_of_objects].rect_width = rect_center_border.Width;
                                listBox1.Items.Add("Объект " + count_of_objects + " " + Found_Objects[count_of_objects].coord_x + " " + Found_Objects[count_of_objects].coord_y);
                                count_of_objects++;
                                goto end;
                            }                           
                            if (i >= pictureBox2.Width)
                            {
                                goto finish;
                            }
                        }
                    }
                }
            }
            end:
            DialogResult dialogresult = MessageBox.Show("", "Объект найден, продолжить поиск?", MessageBoxButtons.YesNo);
            if (dialogresult == DialogResult.Yes)
            {
                obj_found = false;
                radius_min = Int32.Parse(textBox3.Text);
                goto Again;
            }
            else if (dialogresult == DialogResult.No)
            {
                goto finish;
            }
            finish:
            MessageBox.Show("Поиск окончен");
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            pictureBox4.Image = null;
            Bitmap bmp_picture = new Bitmap(pictureBox1.Height, pictureBox1.Width);
            bmp_picture = (Bitmap)Image.FromFile(comboBox1.Text);
            string str = listBox1.SelectedItem.ToString();
            MessageBox.Show(str);
            int ind = listBox1.FindString(str);
            Graphics g = pictureBox2.CreateGraphics();
            Color color = Color.Red; //Отрисовывем границу
            Rectangle rect = new Rectangle(Found_Objects[ind].coord_x, Found_Objects[ind].coord_y, Found_Objects[ind].rect_height, Found_Objects[ind].rect_width);
            g.DrawRectangle(new Pen(color), rect);
            int coord_x = Found_Objects[ind].coord_x;
            int coord_y = Found_Objects[ind].coord_y;
            bmp_object = new Bitmap(Found_Objects[ind].rect_width, Found_Objects[ind].rect_height);
            // выводим выбранный объект в picturebox
            for (int i = 0; i < bmp_object.Width; i++)
            {
                for (int j = 0; j < bmp_object.Height; j++)
                {
                    if (coord_x < pictureBox1.Width)
                    {
                        Color px = bmp_picture.GetPixel(coord_x, coord_y);
                        bmp_object.SetPixel(i, j, px);
                        coord_y++;
                    }
                }
                coord_x++;
                coord_y = Found_Objects[ind].coord_y;
            }
            pictureBox4.SizeMode = PictureBoxSizeMode.Zoom; //сохраняем его и прочие обработки
            pictureBox4.Image = bmp_object;
            string adr = @"C:\C#\objects\" + listBox1.SelectedIndex + ".bmp";
            bmp_object.Save(adr);
            bmp_object = (Bitmap)Image.FromFile(adr);
            button6.Enabled = true;            
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e) //открытие шаблона
        {
            //pictureBox4.Image = null;
            Bitmap bmp_sign = new Bitmap(pictureBox3.Height, pictureBox3.Width);
            pictureBox3.Image = null;
            if (bmp_sign != null)
            {
                bmp_sign.Dispose();
            }
            pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox3.Image = Image.FromFile(comboBox2.Text);
            bmp_sign = (Bitmap)Image.FromFile(comboBox2.Text);
        }

        private void button5_Click(object sender, EventArgs e)                //алгоритм автомтатического поиска, по сути все то же самое, что и в ручном, действия повторяются
        {
            if (comboBox1.Text != "Список файлов в папке" && comboBox1.Text != "" && comboBox2.Text != "" && comboBox2.Text != "База знаков")
            {
                button2_Click(this, e);
                pictureBox1.Image = Image.FromFile(comboBox1.Text);
                bmp_binarization = bmp_filter;
                bmp_binarization = binarization(bmp_binarization, pictureBox1.Width, pictureBox1.Height);
                pictureBox2.Image = bmp_binarization;
                Thread.Sleep(1000);
                listBox1.Items.Clear();
                int count_of_objects = 0;
                bool obj_found = false;
                int x = 0; int y = 0;
                int eq_of_points = 0;
                Bitmap bmp_clast = new Bitmap(pictureBox2.Height, pictureBox2.Width);
                bmp_clast = (Bitmap)pictureBox2.Image;
                Graphics g = pictureBox2.CreateGraphics();
                // переменная поиска объекта
                int pic_border = pictureBox1.Height / 2;                              // макс радиус - половина высоты картинки
                try
                {
                    radius_max = Int32.Parse(textBox4.Text);
                    radius_min = Int32.Parse(textBox3.Text);
                    density = Int32.Parse(textBox2.Text);
                }
                catch (SystemException)
                {
                    string msg = "Введите корректные значения";
                    MessageBox.Show(msg);
                }
                density = check_density(density);                                    // проверки функциями выше
                radius_min = min_radius_check(radius_min, pic_border);
                radius_max = max_radius_check(radius_max, pic_border);
                radius_min = min_max_compare(radius_min, radius_max);
                int coord_multilply = (bmp_clast.Height - radius_min) * (bmp_clast.Width - radius_min);
                int start_i = radius_min;
                int start_j = radius_min;
                textBox2.Text = Convert.ToString(density);
                textBox3.Text = Convert.ToString(radius_min);
                textBox4.Text = Convert.ToString(radius_max);
                //label11.Text = Convert.ToString(bmp_clast.Height);
                //label12.Text = Convert.ToString(bmp_clast.Width);
                Bitmap bmp_save = new Bitmap(pictureBox2.Width, pictureBox2.Height);
                int area = (bmp_clast.Height / 2 - 2 * radius_min - 1) * (bmp_clast.Width / 2 - 2 * radius_min - 1);
                int count_of_iteration = 0;
                Again:
                while (count_of_iteration < area)  // Пока объект не найден лии все пиксели не перебраны
                {
                    for (int i = start_i; i < bmp_clast.Width - radius_min; i = i + 3)
                    {
                        for (int j = start_j; j < bmp_clast.Height - radius_min; j = j + 3)
                        {
                            pictureBox2.Image = bmp_binarization;
                            Rectangle rect2 = new Rectangle(i - radius_min, j - radius_min, 2 * radius_min + 1, 2 * radius_min + 1);
                            int y_border = bmp_clast.Height - 2;
                            int x_border = bmp_clast.Width - 2;
                            int start_x = i - radius_min;                                    //Задание сдвига на радиус
                            int start_y = j - radius_min;
                            int finish_x = i + radius_min - 3;
                            int finish_y = j + radius_min - 3;
                            if (finish_x < x_border && finish_y < y_border)                         // обработка пикселей в заданном квадрате
                            {
                                eq_of_points = 0;
                                if(x>=0&& x<=bmp_clast.Width)
                                {
                                    for (x = start_x; x < finish_x; x++)
                                    {
                                        for (y = start_y; y < finish_y; y++)
                                        {
                                            Color px = bmp_clast.GetPixel(x, y);
                                            if (px.R == 255 && px.G == 255 && px.B == 255)
                                            {
                                                eq_of_points++;                                     //Подсчет белых пикселей
                                            }
                                        }
                                    }
                                    count_of_iteration++;
                                }
                               
                                // визуализация поиска, чтобы посмотреть логику работы, можно расскомментить и показать
                                Color color1 = Color.FromArgb(50, 0, 0, 255);
                                Rectangle rect1 = new Rectangle(i - radius_min, j - radius_min, 2 * radius_min + 1, 2 * radius_min + 1);
                                g.DrawRectangle(new Pen(color1), rect1);
                                int square = ((2 * radius_min + 1) * (2 * radius_min + 1));          //ПОиск площади квадрата
                                double final_density;
                                final_density = (double)eq_of_points / square;                             //расчет финальной плотности
                                final_density = (double)final_density * 100;
                                label10.Text = Convert.ToString(final_density);
                                if (density < final_density)                                               // если расчетная плотность больше заданной
                                {                                                                          //увеличиваем радиус поиска
                                                                                                           // MessageBox.Show("Увеличение радиуса");
                                    start_i = start_i + 10;
                                    start_j = start_j + 10;
                                    radius_min = radius_min + 10;
                                }
                                if (radius_min >= radius_max)                                                     //Если радиус поиска превышает 25
                                {                                                                            // объект найден
                                    MessageBox.Show("Объект найден");
                                    clast_center coordinates_center;
                                    object_border coordinates_border;
                                    coordinates_center = clast_center_find(i - radius_min, j - radius_min, i + 2 * radius_min + 1, j + 2 * radius_min + 1, bmp_binarization);
                                    coordinates_border = find_border(i - radius_min, j - radius_min, i + 2 * radius_min + 1, j + 2 * radius_min + 1, bmp_binarization);
                                    int x_width = coordinates_center.x_center - coordinates_border.x_left;
                                    int y_height = coordinates_center.y_center - coordinates_border.y_up;
                                    Color color = Color.Red;                                                 //Отрисовывем границу
                                    Rectangle rect = new Rectangle(i - radius_min + 10, j - radius_min + 10, 2 * radius_min + 1, 2 * radius_min + 1);
                                    obj_found = true;
                                    Rectangle rect_center_border = new Rectangle(coordinates_border.x_left, coordinates_border.y_up, x_width * 2 + 1, y_height * 2 + 1);
                                    Rectangle rect_cent = new Rectangle(coordinates_center.x_center - 1, coordinates_center.y_center - 1, 3, 3);
                                    g.DrawRectangle(new Pen(color), rect_cent);
                                    g.DrawRectangle(new Pen(Color.GreenYellow), rect_center_border);
                                    i = i + radius_min;
                                    start_j = j + radius_min;
                                    Found_Objects[count_of_objects].coord_x = rect_center_border.X;
                                    Found_Objects[count_of_objects].coord_y = rect_center_border.Y;
                                    Found_Objects[count_of_objects].rect_height = rect_center_border.Height;
                                    Found_Objects[count_of_objects].rect_width = rect_center_border.Width;
                                    listBox1.Items.Add("Объект " + count_of_objects + " " + Found_Objects[count_of_objects].coord_x + " " + Found_Objects[count_of_objects].coord_y);
                                    count_of_objects++;
                                    goto end;
                                }
                                if (count_of_iteration > area)
                                {
                                    goto end;
                                }
                            }
                        }
                    }
                }
                end:
                MessageBox.Show("Поиск окончен");
                for (int obj_ind = 0; obj_ind < listBox1.Items.Count; obj_ind++)
                {
                    pictureBox4.Image = null;
                    Bitmap bmp_picture = new Bitmap(pictureBox1.Height, pictureBox1.Width);
                    bmp_picture = (Bitmap)Image.FromFile(comboBox1.Text);
                    Graphics gr = pictureBox2.CreateGraphics();
                    Color color = Color.Red;                                                 //Отрисовывем границу
                    Rectangle rect = new Rectangle(Found_Objects[obj_ind].coord_x, Found_Objects[obj_ind].coord_y, Found_Objects[obj_ind].rect_height, Found_Objects[obj_ind].rect_width);
                    gr.DrawRectangle(new Pen(color), rect);
                    int coord_x = Found_Objects[obj_ind].coord_x;
                    int coord_y = Found_Objects[obj_ind].coord_y;
                    bmp_object = new Bitmap(Found_Objects[obj_ind].rect_width, Found_Objects[obj_ind].rect_height);
                    for (int i = 0; i < bmp_object.Width; i++)
                    {
                        for (int j = 0; j < bmp_object.Height; j++)
                        {
                            if (coord_x < pictureBox1.Width)
                            {
                                Color px = bmp_picture.GetPixel(coord_x, coord_y);
                                bmp_object.SetPixel(i, j, px);
                                coord_y++;
                            }
                        }
                        coord_x++;
                        coord_y = Found_Objects[obj_ind].coord_y;
                    }
                    pictureBox4.SizeMode = PictureBoxSizeMode.Zoom;
                    pictureBox4.Image = bmp_object;
                    string adr = @"C:\C#\objects\" + listBox1.SelectedIndex + ".bmp";
                    bmp_object.Save(adr);
                    bmp_object = (Bitmap)Image.FromFile(adr);
                }
                if (listBox1.Items.Count > 0)
                {
                    obj_found = true;
                }
                if (obj_found == true)
                {

                    Bitmap bmp_sign = new Bitmap(pictureBox3.Height, pictureBox3.Width);
                    pictureBox3.Image = null;
                    if (bmp_sign != null)
                    {
                        bmp_sign.Dispose();
                    }
                    pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;
                    pictureBox3.Image = Image.FromFile(@"C:\C#\Signs\5_5.gif");
                    bmp_sign = (Bitmap)Image.FromFile(@"C:\C#\Signs\5_5.gif");
                    bmp_sample = (Bitmap)Image.FromFile(@"C:\C#\Signs\5_5.gif");
                    double scale_object_width = (double)pictureBox4.Width / bmp_object.Width;
                    double scale_object_height = (double)pictureBox4.Height / bmp_object.Height;
                    double scale_sample_width = (double)pictureBox3.Width / bmp_sample.Width;
                    double scale_sample_height = (double)pictureBox3.Height / bmp_sample.Height;
                    bmp_object = Resize_bmp(bmp_object, scale_object_width, scale_object_height);
                    bmp_sample = Resize_bmp(bmp_sample, scale_sample_width, scale_sample_height);
                    pictureBox4.Image = (Image)bmp_object;
                    pictureBox3.Image = (Image)bmp_sample;
                    bmp_object.Save(@"C:\C#\object.bmp");
                    bmp_sample.Save(@"C:\C#\sample.bmp");
                    bmp_sample = binarization(bmp_sample, bmp_sample.Width, bmp_sample.Height);
                    bmp_object = binarization(bmp_object, bmp_object.Width, bmp_object.Height);
                    pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;
                    pictureBox4.SizeMode = PictureBoxSizeMode.Zoom;
                    double compare_coefficient = 0;
                    for (int i = 0; i < bmp_object.Width; i++)
                    {
                        for (int j = 0; j < bmp_object.Height; j++)
                        {
                            Color px_obj = bmp_object.GetPixel(i, j);
                            Color px_sample = bmp_sample.GetPixel(i, j);
                            if ((px_obj.R != 0 || px_obj.G != 0 || px_obj.B != 0) && (px_sample.R != 0 || px_sample.G != 0 || px_sample.B != 0))
                            {
                                bmp_object.SetPixel(i, j, Color.LightGreen);
                                compare_coefficient++;

                            }
                            else
                            {
                                if ((px_obj.R != 0 || px_obj.G != 0 || px_obj.B != 0) && (px_sample.R == 0 || px_sample.G == 0 || px_sample.B == 0))
                                {
                                    bmp_object.SetPixel(i, j, Color.LightCoral);
                                }
                                else
                                {
                                    if ((px_obj.R == 0 || px_obj.G == 0 || px_obj.B == 0) && (px_sample.R != 0 || px_sample.G != 0 || px_sample.B != 0))
                                    {
                                        bmp_object.SetPixel(i, j, Color.DarkRed);
                                    }
                                    else
                                    {
                                        bmp_object.SetPixel(i, j, Color.DarkGreen);
                                        compare_coefficient++;

                                    }
                                }
                            }

                        }
                    }
                    compare_coefficient = compare_coefficient / (pictureBox4.Width * pictureBox4.Height);
                    compare_coefficient = compare_coefficient * 100;
                    label11.Text = Convert.ToString(compare_coefficient);
                    if (compare_coefficient > 80)
                    {
                        MessageBox.Show("Объект найден, коэффицент соответствия " + label11.Text + ". Объект соответствует выбранному знаку");
                    }
                    else
                    {
                        MessageBox.Show("Объект найден, коэффицент соответствия " + label11.Text + ". Объект не соответствует выбранному знаку");
                    }
                    
                }
                else
                {
                    MessageBox.Show("Объекты не найдены");
                }
            }
            else
            {
                MessageBox.Show("Выбран файл загрузки и знак из базы");
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)    //вывод цвета пискесля, просто дебажная фигня
        {
            Bitmap bmp_Color = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.DrawToBitmap(bmp_Color, new Rectangle(Point.Empty, pictureBox1.Size));
            Color pix = bmp_Color.GetPixel(e.X, e.Y);


            label15.Text = Convert.ToString(pix.A);
            label16.Text = Convert.ToString(pix.R);
            label17.Text = Convert.ToString(pix.G);
            label18.Text = Convert.ToString(pix.B);
        }

        private void button6_Click(object sender, EventArgs e)      //сравнение двух объектов
        {
            if (comboBox2.SelectedItem == null)
            {
                MessageBox.Show("Выберите картинку");
            }
            else
            {
                bmp_sample = (Bitmap)Image.FromFile(comboBox2.Text);
                double scale_object_width = (double)pictureBox4.Width / bmp_object.Width;    //расчет коэффициентов масштабирования для объекта и шаблона
                double scale_object_height = (double)pictureBox4.Height / bmp_object.Height;
                double scale_sample_width = (double)pictureBox3.Width / bmp_sample.Width;
                double scale_sample_height = (double)pictureBox3.Height / bmp_sample.Height;
                bmp_object = Resize_bmp(bmp_object, scale_object_width, scale_object_height);   //Мастшабирование
                bmp_sample = Resize_bmp(bmp_sample, scale_sample_width, scale_sample_height);
                pictureBox4.Image = (Image)bmp_object;
                pictureBox3.Image = (Image)bmp_sample;
                bmp_object.Save(@"C:\C#\objects\object.bmp");                                        //сохранение
                bmp_sample.Save(@"C:\C#\objects\sample.bmp");
                bmp_sample = binarization(bmp_sample, bmp_sample.Width, bmp_sample.Height);      //бинаризация
                bmp_object = binarization(bmp_object, bmp_object.Width, bmp_object.Height);
                pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;                                   //вывод бинаризованных ихображений
                pictureBox4.SizeMode = PictureBoxSizeMode.Zoom;
                double compare_coefficient = 0;
                for (int i = 0; i < bmp_object.Width; i++)
                {
                    for (int j = 0; j < bmp_object.Height; j++)
                    {
                        Color px_obj = bmp_object.GetPixel(i, j);
                        Color px_sample = bmp_sample.GetPixel(i, j);
                        if ((px_obj.R != 0 || px_obj.G != 0 || px_obj.B != 0) && (px_sample.R != 0 || px_sample.G != 0 || px_sample.B != 0))
                        {
                            bmp_object.SetPixel(i, j, Color.LightGreen);
                            compare_coefficient++;

                        }
                        else
                        {
                            if ((px_obj.R != 0 || px_obj.G != 0 || px_obj.B != 0) && (px_sample.R == 0 || px_sample.G == 0 || px_sample.B == 0))
                            {
                                bmp_object.SetPixel(i, j, Color.LightCoral);
                            }
                            else
                            {
                                if ((px_obj.R == 0 || px_obj.G == 0 || px_obj.B == 0) && (px_sample.R != 0 || px_sample.G != 0 || px_sample.B != 0))
                                {
                                    bmp_object.SetPixel(i, j, Color.DarkRed);
                                }
                                else
                                {
                                    bmp_object.SetPixel(i, j, Color.DarkGreen);
                                    compare_coefficient++;

                                }
                            }
                        }

                    }
                }
                compare_coefficient = compare_coefficient / (pictureBox4.Width * pictureBox4.Height);
                compare_coefficient = compare_coefficient * 100;
                label11.Text = Convert.ToString(compare_coefficient);
                if (compare_coefficient > 80)
                {
                    MessageBox.Show("Объект найден, коэффицент соответствия " + label11.Text + ". Объект соответствует выбранному знаку");
                }
                else
                {
                    MessageBox.Show("Объект найден, коэффицент соответствия " + label11.Text + ". Объект не соответствует выбранному знаку");
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button2.Enabled = true;
            //Отрисовка
            pictureBox1.Image = null;
            pictureBox2.Image = null;
            if (bmp_filter != null)
            {
                bmp_filter.Dispose();
            }
            if (bmp_bin_start_pic != null)
            {
                bmp_bin_start_pic.Dispose();
            }
            if (bmp_binarization != null)
            {
                bmp_binarization.Dispose();
            }
            pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;
            pictureBox2.SizeMode = PictureBoxSizeMode.AutoSize;
            pictureBox1.Image = Image.FromFile(comboBox1.Text);
            int i = pictureBox1.Width;
            int j = pictureBox1.Height;
            bmp_start = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            bmp_filter = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            bmp_binarization = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            bmp3 = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            bmp_filter = (Bitmap)Image.FromFile(comboBox1.Text);
            bmp_start = (Bitmap)Image.FromFile(comboBox1.Text);
            point_eq = pictureBox1.Width * pictureBox1.Height;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                //checkBox1.Text = "Включить ручной";
                button2.Enabled = false;
                button3.Enabled = false;
                button4.Enabled = false;
                button6.Enabled = false;
                button5.Enabled = true;
            }
            else
            {
                //checkBox1.Text = "Включить автоматический";
                button2.Enabled = true;
                button5.Enabled = false;
            }
        }
    }
}