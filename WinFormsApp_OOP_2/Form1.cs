using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Serialization;
using WinFormsApp_OOP_1.GraphicsFigures.Figures;
using WinFormsApp_OOP_2.Drawers;
using WinFormsApp_OOP_2.Visitors;

namespace WinFormsApp_OOP_2
{
    public partial class Form1 : Form
    {
        private List<IShapePlugin> _plugins = new List<IShapePlugin>();
        private ShapeProcessor _shapeProcessor = new ShapeProcessor(new List<IShapePlugin>());

        public Form1()
        {
            InitializeComponent();
        }

        private GraphicsVisitor graphicsVisitor;
        private List<IFigure> figuresList = new List<IFigure>();

        ArchivatorAdapter archivator;

        XmlSerializer serializer;
        IFigure selectedShape;

        private System.Drawing.Point startPoint;
        private System.Drawing.Point endPoint;

        private void Form1_Load(object sender, EventArgs e)
        {
            listBox.Items.Add(new ComboboxItem() { Text = "Circle", Value = new CircleFactory() });
            listBox.Items.Add(new ComboboxItem() { Text = "Ellipse", Value = new EllipseFactory() });
            listBox.Items.Add(new ComboboxItem() { Text = "Line", Value = new LineFactory() });
            listBox.Items.Add(new ComboboxItem() { Text = "Point", Value = new PointFactory() });
            listBox.Items.Add(new ComboboxItem() { Text = "Quadrilateral", Value = new QuadrilateralFactory() });
            listBox.Items.Add(new ComboboxItem() { Text = "Rectangle", Value = new RectangleFactory() });
            listBox.Items.Add(new ComboboxItem() { Text = "Square", Value = new SquareFactory() });

        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                startPoint = e.Location;
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Left)
            {
                endPoint = e.Location;

                IFactory? facroty = (listBox.SelectedItem as ComboboxItem)?.Value as IFactory;
                IFigure figure = facroty.Create(startPoint, endPoint);

                figuresList.Add(figure);
                listBox1.Items.Add(figure);

                listBox1.SelectedItem = figure;

                pictureBox1.Invalidate();

            }
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            graphicsVisitor = new GraphicsVisitor(e.Graphics);

            foreach (IFigure figure in figuresList)
            {
                figure.Accept(graphicsVisitor);
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (selectedShape != null)
                selectedShape.IsSelected = false;

            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                Formatting = Formatting.Indented

            };

            var json = JsonConvert.SerializeObject(figuresList, settings);
            File.WriteAllText("C:\\Users\\Dimaland\\Documents\\1\\figures.json", json);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string json = File.ReadAllText("C:\\Users\\Dimaland\\Documents\\1\\figures.json");

            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                Formatting = Formatting.Indented

            };

            figuresList = JsonConvert.DeserializeObject<List<IFigure>>(json, settings);

            foreach (IFigure figure in figuresList)
            {
                listBox1.Items.Add(figure);
            }

            pictureBox1.Invalidate();
        }

        private void propertyGrid1_SelectedObjectsChanged(object sender, EventArgs e)
        {
            if (selectedShape != null)
            {
                selectedShape.IsSelected = false;
            }

            selectedShape = propertyGrid1.SelectedObject as IFigure;

            if (selectedShape != null)
            {
                selectedShape.IsSelected = true;
            }

            pictureBox1.Invalidate();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            propertyGrid1.SelectedObject = listBox1.SelectedItem;
        }

        private void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (listBox1.SelectedItem is IFigure selected)
                {
                    figuresList.Remove(selected);
                    listBox1.Items.Remove(selected);

                    pictureBox1.Invalidate();
                }

            }
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (e.ChangedItem.PropertyDescriptor.Name == "FigureColor" || e.ChangedItem.PropertyDescriptor.Name == "OutlineColor")
            {
                selectedShape.IsSelected = false;
            }

            pictureBox1.Invalidate();

        }

        // �����, �������������� ��������� ��� ������������
        [XmlRoot("Figures")]
        public class FigureContainer
        {
            [XmlElement("Figure")]
            public List<IFigure>? FiguresList { get; set; }
        }


        private void button3_Click(object sender, EventArgs e)
        {
            if (selectedShape != null)
                selectedShape.IsSelected = false;

            FigureContainer container = new FigureContainer { FiguresList = figuresList };

            var knownTypes = FigureTypeLoader.LoadFigureTypes("C:\\Users\\Dimaland\\source\\repos\\Dimaland16\\WinFormsApp_OOP_2\\WinFormsLibrary1\\bin\\Debug\\net8.0-windows");

            // ����������� � ��������� � XML ����
            serializer = new XmlSerializer(typeof(FigureContainer), knownTypes.ToArray());
            using (TextWriter writer = new StreamWriter("C:\\Users\\Dimaland\\Documents\\1\\figures.xml"))
            {
                serializer.Serialize(writer, container);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var knownTypes = FigureTypeLoader.LoadFigureTypes("C:\\Users\\Dimaland\\source\\repos\\Dimaland16\\WinFormsApp_OOP_2\\WinFormsLibrary1\\bin\\Debug\\net8.0-windows");

            // ������������� �� XML �����
            serializer = new XmlSerializer(typeof(FigureContainer), knownTypes.ToArray());

            FigureContainer deserializedContainer;
            using (TextReader reader = new StreamReader("C:\\Users\\Dimaland\\Documents\\1\\figures.xml"))
            {
                deserializedContainer = (FigureContainer)serializer.Deserialize(reader);
            }

            figuresList = deserializedContainer.FiguresList;

            foreach (IFigure figure in figuresList)
            {
                listBox1.Items.Add(figure);
            }

            pictureBox1.Invalidate();

        }

        private void button6_Click(object sender, EventArgs e)
        {
            figuresList.Clear();
            listBox1.Items.Clear();
            propertyGrid1.SelectedObject = null;
            pictureBox1.Invalidate();

        }

        private void Form1_Click(object sender, EventArgs e)
        {
            if (selectedShape != null)
            {
                selectedShape.IsSelected = false;
            }
            listBox1.SelectedItem = null;
            propertyGrid1.SelectedObject = null;

            pictureBox1.Invalidate();

        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "DLL files (*.dll)|*.dll|All files (*.*)|*.*";
                openFileDialog.InitialDirectory = "C:\\Users\\Dimaland\\source\\repos\\Dimaland16\\WinFormsApp_OOP_2\\WinFormsLibrary1\\bin\\Debug\\net8.0-windows";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string assemblyPath = openFileDialog.FileName;
                    LoadAndRegisterShapeLibrary(assemblyPath);
                }
            }
        }

        private void LoadAndRegisterShapeLibrary(string assemblyPath)
        {
            Assembly newShapesAssembly = Assembly.LoadFrom(assemblyPath);

            var factoryTypes = newShapesAssembly.GetTypes().Where(t => typeof(IFactory).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var factoryType in factoryTypes)
            {
                IFactory factory = (IFactory)Activator.CreateInstance(factoryType);

                string shapeTypeName = factoryType.Name.Replace("Factory", "");
                Type shapeType = newShapesAssembly.GetType($"WinFormsLibrary1.{shapeTypeName}");

                IFigure shape = factory.Create(startPoint, endPoint);

                string drawMethodName = $"{shapeTypeName}DrawMethod";
                MethodInfo drawMethod = newShapesAssembly.GetType($"WinFormsLibrary1.{drawMethodName}").GetMethod("Draw");

                graphicsVisitor.RegisterDrawMethod(shape.GetType(), (graphics, s) => drawMethod.Invoke(null, new object[] { graphics, s }));

                listBox.Items.Add(new ComboboxItem() { Text = shapeTypeName, Value = factory });
            }
        }


        private void button8_Click(object sender, EventArgs e)
        {
            string pluginDirectory = "C:\\Users\\Dimaland\\source\\repos\\Dimaland16\\WinFormsApp_OOP_2\\WinFormsLibrary2\\bin\\Debug\\net8.0-windows";


            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Assembly Files|*.dll";
            openFileDialog.InitialDirectory = pluginDirectory;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var pluginPath = openFileDialog.FileName;
                var loadedPlugins = PluginLoader.LoadPlugins(Path.GetDirectoryName(pluginPath));

                _plugins.AddRange(loadedPlugins);
                _shapeProcessor = new ShapeProcessor(_plugins);

                foreach (var plugin in _plugins)
                {
                    var menuItem = new ToolStripMenuItem(plugin.Name);
                    pluginsToolStripMenuItem.DropDownItems.Add(menuItem);
                }
            }

        }

        private void jsonDeserialization_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (selectedShape != null)
                    selectedShape.IsSelected = false;

                figuresList = _shapeProcessor.LoadShapes(openFileDialog.FileName);

                foreach (IFigure figure in figuresList)
                {
                    listBox1.Items.Add(figure);
                }

                pictureBox1.Invalidate();
            }
        }

        private void jsonSerialization_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (selectedShape != null)
                    selectedShape.IsSelected = false;

                _shapeProcessor.SaveShapes(figuresList, saveFileDialog.FileName);
            }
        }

        private void buttonLoadPlugin_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "DLL files (*.dll)|*.dll|All files (*.*)|*.*",
                InitialDirectory = "C:\\Users\\Dimaland\\Downloads\\UserArchievePlugin (1)\\UserArchievePlugin\\bin\\Debug\\net7.0-windows"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string assemblyPath = openFileDialog.FileName;
                try
                {
                    archivator = new ArchivatorAdapter(assemblyPath);
                    MessageBox.Show("Plugin loaded successfully!");

                    ToolStripMenuItem functionsToolMenuItem = new ToolStripMenuItem(archivator.ToString());

                    ToolStripMenuItem saveToZipMenuItem = new ToolStripMenuItem("Save to ZIP");
                    saveToZipMenuItem.Click += SaveToZipMenuItem_Click;

                    ToolStripMenuItem openZipMenuItem = new ToolStripMenuItem("Open ZIP");
                    openZipMenuItem.Click += OpenZipMenuItem_Click;

                    functionsToolMenuItem.DropDownItems.Add(saveToZipMenuItem);
                    functionsToolMenuItem.DropDownItems.Add(openZipMenuItem);

                    pluginsToolStripMenuItem.DropDownItems.Add(functionsToolMenuItem);

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load plugin: {ex.Message}");
                }
            }
        }

        private void SaveToZipMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "ZIP files (*.zip)|*.zip|All files (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = "C:\\Users\\Dimaland\\Documents\\1\\figures.xml";
                string zipFilePath = saveFileDialog.FileName;

                try
                {
                    archivator.ArchiveXmlFile(filePath, zipFilePath);
                    MessageBox.Show($"File archived to: {zipFilePath}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to archive file: {ex.Message}");
                }
            }
        }

        private void OpenZipMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "ZIP files (*.zip)|*.zip|All files (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string zipFilePath = openFileDialog.FileName;
                string extractPath = "C:\\Users\\Dimaland\\Documents\\ExtractedFiles";

                try
                {
                    archivator.UnzipArchive(zipFilePath, extractPath);
                    MessageBox.Show($"Archive extracted to: {extractPath}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to extract archive: {ex.Message}");
                }
            }
        }
    }
}
