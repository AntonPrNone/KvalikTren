using System.Collections.Generic;
using System.Data;
using System.Windows.Controls;
using System.Windows.Media;
using KvalikTren.Model;
using QRCoder;
using QRCoder.Xaml;
using System.Windows;
using System.Windows.Data;
using System.Linq;

namespace KvalikTren
{
    /// <summary>
    /// Логика взаимодействия для UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        DataTable dataTable;
        public UserControl1()
        {
            InitializeComponent();

            dataTable = new DataTable("Products");
            InitializeDataGridColumns();
            LoadDataGrid();
        }


        private void LoadDataGrid()
        {
            List<Product> products = DB_Interaction.GetProduct();

            dataTable = new DataTable("Products");
            dataTable.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("ID", typeof(int)),
                new DataColumn("Наименование", typeof(string)),
                new DataColumn("Кол-во", typeof(int)),
                new DataColumn("Тип", typeof(string)),
                new DataColumn("QR Code", typeof(DrawingImage))
            });

            foreach (Product product in products)
            {
                DrawingImage qrCodeImage = GenerateQRCodeImage(product);
                DataRow row = dataTable.NewRow();
                row["ID"] = product.ID_Products;
                row["Наименование"] = product.Name;
                row["Кол-во"] = product.Count;
                row["Тип"] = product.Type;
                row["QR Code"] = qrCodeImage;

                dataTable.Rows.Add(row);
            }

            Product_DataGrid.ItemsSource = dataTable.DefaultView;
        }


        private void InitializeDataGridColumns()
        {
            DataGridTemplateColumn qrCodeColumn = new DataGridTemplateColumn();
            qrCodeColumn.Header = "QR Code";
            FrameworkElementFactory factory = new FrameworkElementFactory(typeof(Image));
            factory.SetBinding(Image.SourceProperty, new Binding("QR Code"));
            qrCodeColumn.CellTemplate = new DataTemplate { VisualTree = factory };
            qrCodeColumn.IsReadOnly = true;
            qrCodeColumn.Width = 100;
            Product_DataGrid.Columns.Add(qrCodeColumn);
        }

        private DrawingImage GenerateQRCodeImage(Product product)
        {
            string sourceData = $"ID: {product.ID_Products}\nНазвание: {product.Name}\nКоличество: {product.Count}\nТип: {product.Type}";

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(sourceData, QRCodeGenerator.ECCLevel.Q);
            XamlQRCode qrCode = new XamlQRCode(qrCodeData);
            DrawingImage bitmap = qrCode.GetGraphic(100);
            return bitmap;
        }

        private void Student_DataGrid_InitializingNewItem(object sender, InitializingNewItemEventArgs e)
        {
            // Получаем максимальное значение ID в текущем DataTable
            int maxId = dataTable.AsEnumerable().Max(row => row.Field<int>("ID"));

            // Увеличиваем на 1 и присваиваем новое значение для следующей строки
            int newId = maxId + 1;

            // Присваиваем новое значение полю ID в текущей добавляемой строке
            DataRowView newRow = e.NewItem as DataRowView;
            if (newRow != null)
            {
                newRow["ID"] = newId;
            }
        }

        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            DB_Interaction.UpdateProducts(dataTable);
            LoadDataGrid();
        }
    }
}
