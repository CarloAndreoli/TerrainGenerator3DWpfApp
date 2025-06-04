using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace terrainGeneratorSample
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{

		/**
		 * <summary>
		 * Constructor
		 * </summary>
		 */
		public MainWindow()
		{
			InitializeComponent();
			Activated += _MainWindowActivated;
		}


		/**
		 * <summary>
		 * property to know if the windows has been showed once
		 * </summary>
		 */
		private bool _Show = false;

		/**
		 * <summary>
		 * Method raised on Activated event
		 * </summary>
		 */
		private void _MainWindowActivated(object sender, EventArgs e)
		{
			if (!_Show) {
				_GenerateTerrainButtonClick(null, null);
				_Show = true;
			}
		}

		/**
		 * <summary>
		 * Method called on 'generate' button
		 * </summary>
		 *
		 * <param name="sender">sender object</param>
		 * <param name="RoutedEventArgs">arguments</param>
		 */
		private void _GenerateTerrainButtonClick(object sender, RoutedEventArgs e)
		{
			int detailValue = (int)System.Math.Round(slider_detail.Value);
			float roughnessValue = (float)slider_roughness.Value;

			//clean up
			_MyModel3DGroup.Children.Clear();

			//generate terrain
			TerrainGenerator tg = new TerrainGenerator(detailValue);
			tg.Generate(roughnessValue);
			
			PointLight pointLight = new PointLight(Colors.White, new Point3D(tg.Size / 2, tg.Size / 2, tg.Size * 3 / 5));
			_MyModel3DGroup.Children.Add(pointLight);

			float minHeightValue = 0;
			float maxHeightValue = 0;
			for (var y = 0; y < tg.Size; y++) {
				for (var x = 0; x < tg.Size; x++) {
					float val = tg.Map[x, y];
					if (val < minHeightValue) minHeightValue = val;
					if (val > maxHeightValue) maxHeightValue = val;
				}
			}

			float waterVal = (float)(tg.Size * 0.5);

			int cellSize = 4096;
			for (int x = 0; x < tg.Size - 1; x += cellSize) {
				for (int y = 0; y < tg.Size - 1; y += cellSize) {
					int maxX = x + cellSize + 1;
					int maxY = y + cellSize + 1;

					if (maxX > tg.Size) maxX = tg.Size;
					if (maxY > tg.Size) maxY = tg.Size;
					
					_DrawTerrain(tg.Map, tg.Size, minHeightValue, maxHeightValue, x, y, maxX, maxY);

					System.GC.Collect();
					System.GC.WaitForFullGCComplete();
				}
			}
			_DrawWater(tg.Map, tg.Size, minHeightValue, maxHeightValue, waterVal);
			_DrawBox(tg.Map, tg.Size, minHeightValue, maxHeightValue, waterVal);
		}


		/**
		 * <summary>
		 * Method that create a water effect for the terrain
		 * </summary>
		 *
		 * <param name="terrainMap">terrain to show</param>
		 * <param name="terrainSize">terrain size</param>
		 * <param name="minHeightValue">minimum terraing height</param>
		 * <param name="maxHeightValue">maximum terraing height</param>
		 * <param name="waterHeightValue">water height value</param>
		 */
		private void _DrawWater(float[,] terrainMap, int terrainSize, float minHeightValue, float maxHeightValue, float waterHeightValue)
		{
			float halfSize = terrainSize / 2;
			float halfheight = (maxHeightValue - minHeightValue) / 2;

			// creation of the water layers
			// I'm going to use a series of emissive layer for water
			SolidColorBrush waterSolidColorBrush = new SolidColorBrush(Colors.Blue);
			waterSolidColorBrush.Opacity = 0.2;
			GeometryModel3D myWaterGeometryModel = new GeometryModel3D(new MeshGeometry3D(), new EmissiveMaterial(waterSolidColorBrush));
			Point3DCollection waterPoint3DCollection = new Point3DCollection();
			Int32Collection triangleIndices = new Int32Collection();

			int triangleCounter = 0;
			float dfMul = 5;
			for (int i = 0; i < 10; i++) {

				triangleCounter = waterPoint3DCollection.Count;

				waterPoint3DCollection.Add(new Point3D(-halfSize, -halfSize, waterHeightValue - i * dfMul - halfheight));
				waterPoint3DCollection.Add(new Point3D(+halfSize, +halfSize, waterHeightValue - i * dfMul - halfheight));
				waterPoint3DCollection.Add(new Point3D(-halfSize, +halfSize, waterHeightValue - i * dfMul - halfheight));
				waterPoint3DCollection.Add(new Point3D(+halfSize, -halfSize, waterHeightValue - i * dfMul - halfheight));

				triangleIndices.Add(triangleCounter);
				triangleIndices.Add(triangleCounter + 1);
				triangleIndices.Add(triangleCounter + 2);
				triangleIndices.Add(triangleCounter);
				triangleIndices.Add(triangleCounter + 3);
				triangleIndices.Add(triangleCounter + 1);
			}
			
			((MeshGeometry3D)myWaterGeometryModel.Geometry).Positions = waterPoint3DCollection;
			((MeshGeometry3D)myWaterGeometryModel.Geometry).TriangleIndices = triangleIndices;
			_MyModel3DGroup.Children.Add(myWaterGeometryModel);
		}

		/**
		 * <summary>
		 * Method that create the 3d terrain on a Viewport3D control
		 * </summary>
		 *
		 * <param name="terrainMap">terrain to show</param>
		 * <param name="terrainSize">terrain size</param>
		 * <param name="minHeightValue">minimum terraing height</param>
		 * <param name="maxHeightValue">maximum terraing height</param>
		 */
		private void _DrawTerrain(float[,] terrainMap, int terrainSize, float minHeightValue, float maxHeightValue)
		{
			_DrawTerrain(terrainMap, terrainSize, minHeightValue, maxHeightValue, 0, 0, terrainSize, terrainSize);
		}


		/**
		 * <summary>
		 * Method that create the 3d terrain on a Viewport3D control
		 * </summary>
		 *
		 * <param name="terrainMap">terrain to show</param>
		 * <param name="terrainSize">terrain size</param>
		 * <param name="minHeightValue">minimum terraing height</param>
		 * <param name="maxHeightValue">maximum terraing height</param>
		 * <param name="posX">position to start the draw along x of the map</param>
		 * <param name="posY">position to start the draw along y of the map</param>
		 * <param name="maxPosX">position till which x coordinate can draw along the map</param>
		 * <param name="maxPosY">position till which y coordinate can draw along the map</param>
		 */
		private void _DrawTerrain(float[,] terrainMap, int terrainSize, float minHeightValue, float maxHeightValue, int posX, int posY, int maxPosX, int maxPosY)
		{
			float halfSize = terrainSize / 2;
			float halfheight = (maxHeightValue - minHeightValue) / 2;

			// creation of the terrain
			GeometryModel3D myTerrainGeometryModel = new GeometryModel3D(new MeshGeometry3D(), new DiffuseMaterial(new SolidColorBrush(Colors.GreenYellow)));
			Point3DCollection point3DCollection = new Point3DCollection();
			Int32Collection triangleIndices = new Int32Collection();
			
			//adding point
			for (var y = posY; y < maxPosY; y++) {
				for (var x = posX; x < maxPosX; x++) {
					point3DCollection.Add(new Point3D(x - halfSize, y - halfSize, terrainMap[x, y] - halfheight));
				}
			}
			((MeshGeometry3D)myTerrainGeometryModel.Geometry).Positions = point3DCollection;

			//defining triangles
			int ind1 = 0;
			int ind2 = 0;
			int xLenght = maxPosX - posX;
			for (var y = 0; y < maxPosY - posY - 1; y++) {
				for (var x = 0; x < maxPosX - posX - 1; x++) {
					ind1 = x + y * (xLenght);
					ind2 = ind1 + (xLenght);

					//first triangle
					triangleIndices.Add(ind1);
					triangleIndices.Add(ind2 + 1);
					triangleIndices.Add(ind2);

					//second triangle
					triangleIndices.Add(ind1);
					triangleIndices.Add(ind1 + 1);
					triangleIndices.Add(ind2 + 1);
				}
			}
			((MeshGeometry3D)myTerrainGeometryModel.Geometry).TriangleIndices = triangleIndices;

			_MyModel3DGroup.Children.Add(myTerrainGeometryModel);
		}


		/**
		 * <summary>
		 * Method that create a box container for the terrain
		 * </summary>
		 *
		 * <param name="terrainMap">terrain to show</param>
		 * <param name="terrainSize">terrain size</param>
		 * <param name="minHeightValue">minimum terraing height</param>
		 * <param name="maxHeightValue">maximum terraing height</param>
		 * <param name="waterHeightValue">water height value</param>
		 */
		private void _DrawBox(float[,] terrainMap, int terrainSize, float minHeightValue, float maxHeightValue, float waterHeightValue)
		{
			float halfSize = terrainSize / 2;
			float halfheight = (maxHeightValue - minHeightValue) / 2;

			// creation of an external box that will contain the object
			GeometryModel3D myBoxGeometryModel = new GeometryModel3D(new MeshGeometry3D(), new DiffuseMaterial(new SolidColorBrush(Colors.Black)));
			Point3DCollection boxPoint3DCollection = new Point3DCollection();
			Int32Collection triangleIndices = new Int32Collection();

			// bottom layer
			boxPoint3DCollection.Add(new Point3D(-halfSize, -halfSize, minHeightValue - halfheight));
			boxPoint3DCollection.Add(new Point3D(-halfSize, +halfSize, minHeightValue - halfheight));
			boxPoint3DCollection.Add(new Point3D(+halfSize, +halfSize, minHeightValue - halfheight));
			boxPoint3DCollection.Add(new Point3D(+halfSize, -halfSize, minHeightValue - halfheight));
			triangleIndices.Add(0);
			triangleIndices.Add(1);
			triangleIndices.Add(2);
			triangleIndices.Add(3);
			triangleIndices.Add(0);
			triangleIndices.Add(2);

			// ddc layer
			boxPoint3DCollection.Add(new Point3D(-halfSize, -halfSize - 0.5, minHeightValue - halfheight));
			boxPoint3DCollection.Add(new Point3D(+halfSize, -halfSize - 0.5, minHeightValue - halfheight));
			boxPoint3DCollection.Add(new Point3D(-halfSize, -halfSize - 0.5, waterHeightValue - halfheight));
			boxPoint3DCollection.Add(new Point3D(+halfSize, -halfSize - 0.5, waterHeightValue - halfheight));
			triangleIndices.Add(4);
			triangleIndices.Add(5);
			triangleIndices.Add(6);
			triangleIndices.Add(6);
			triangleIndices.Add(5);
			triangleIndices.Add(7);

			boxPoint3DCollection.Add(new Point3D(-halfSize - 0.1, -halfSize, minHeightValue - halfheight));
			boxPoint3DCollection.Add(new Point3D(-halfSize - 0.1, -halfSize, waterHeightValue - halfheight));
			boxPoint3DCollection.Add(new Point3D(-halfSize - 0.1, +halfSize, minHeightValue - halfheight));
			boxPoint3DCollection.Add(new Point3D(-halfSize - 0.1, +halfSize, waterHeightValue - halfheight));
			triangleIndices.Add(8);
			triangleIndices.Add(9);
			triangleIndices.Add(10);
			triangleIndices.Add(9);
			triangleIndices.Add(11);
			triangleIndices.Add(10);

			int triangleCounter = 0;
			// layers along y = 0 and y = _Size
			for (var x = 0; x < terrainSize - 1; x++) {

				double valX = terrainMap[x, 0];
				double valX1 = terrainMap[x + 1, 0];
				if (valX < waterHeightValue) valX = waterHeightValue;
				if (valX1 < waterHeightValue) valX1 = waterHeightValue;

				triangleCounter = boxPoint3DCollection.Count;
				boxPoint3DCollection.Add(new Point3D(x - halfSize, -halfSize, minHeightValue - halfheight));
				boxPoint3DCollection.Add(new Point3D(x + 1 - halfSize, -halfSize, minHeightValue - halfheight));
				boxPoint3DCollection.Add(new Point3D(x - halfSize, -halfSize, valX - halfheight));
				boxPoint3DCollection.Add(new Point3D(x + 1 - halfSize, -halfSize, valX1 - halfheight));
				triangleIndices.Add(triangleCounter);
				triangleIndices.Add(triangleCounter + 1);
				triangleIndices.Add(triangleCounter + 2);
				triangleIndices.Add(triangleCounter + 2);
				triangleIndices.Add(triangleCounter + 1);
				triangleIndices.Add(triangleCounter + 3);

				int dy = terrainSize - 1;
				double valXdY = terrainMap[x, dy];
				double valX1dY = terrainMap[x + 1, dy];
				if (valXdY < waterHeightValue) valXdY = waterHeightValue;
				if (valX1dY < waterHeightValue) valX1dY = waterHeightValue;

				triangleCounter = boxPoint3DCollection.Count;

				boxPoint3DCollection.Add(new Point3D(x - halfSize, dy - halfSize, minHeightValue - halfheight));
				boxPoint3DCollection.Add(new Point3D(x - halfSize, dy - halfSize, valXdY - halfheight));
				boxPoint3DCollection.Add(new Point3D(x + 1 - halfSize, dy - halfSize, minHeightValue - halfheight));
				boxPoint3DCollection.Add(new Point3D(x + 1 - halfSize, dy - halfSize, valX1dY - halfheight));

				triangleIndices.Add(triangleCounter);
				triangleIndices.Add(triangleCounter + 1);
				triangleIndices.Add(triangleCounter + 2);
				triangleIndices.Add(triangleCounter + 1);
				triangleIndices.Add(triangleCounter + 3);
				triangleIndices.Add(triangleCounter + 2);
			}
			// layers along x = 0 and x = _Size
			for (var y = 0; y < terrainSize - 1; y++) {

				double valY = terrainMap[0, y];
				double valY1 = terrainMap[0, y + 1];
				if (valY < waterHeightValue) valY = waterHeightValue;
				if (valY1 < waterHeightValue) valY1 = waterHeightValue;

				triangleCounter = boxPoint3DCollection.Count;

				boxPoint3DCollection.Add(new Point3D(-halfSize, y - halfSize, minHeightValue - halfheight));
				boxPoint3DCollection.Add(new Point3D(-halfSize, y - halfSize, valY - halfheight));
				boxPoint3DCollection.Add(new Point3D(-halfSize, y + 1 - halfSize, minHeightValue - halfheight));
				boxPoint3DCollection.Add(new Point3D(-halfSize, y + 1 - halfSize, valY1 - halfheight));

				triangleIndices.Add(triangleCounter);
				triangleIndices.Add(triangleCounter + 1);
				triangleIndices.Add(triangleCounter + 2);
				triangleIndices.Add(triangleCounter + 1);
				triangleIndices.Add(triangleCounter + 3);
				triangleIndices.Add(triangleCounter + 2);

				int dx = terrainSize - 1;
				double valYdX = terrainMap[dx, y];
				double valY1dX = terrainMap[dx, y + 1];
				if (valYdX < waterHeightValue) valYdX = waterHeightValue;
				if (valY1dX < waterHeightValue) valY1dX = waterHeightValue;

				triangleCounter = boxPoint3DCollection.Count;

				boxPoint3DCollection.Add(new Point3D(dx - halfSize, y - halfSize, minHeightValue - halfheight));
				boxPoint3DCollection.Add(new Point3D(dx - halfSize, y + 1 - halfSize, minHeightValue - halfheight));
				boxPoint3DCollection.Add(new Point3D(dx - halfSize, y - halfSize, valYdX - halfheight));
				boxPoint3DCollection.Add(new Point3D(dx - halfSize, y + 1 - halfSize, valY1dX - halfheight));

				triangleIndices.Add(triangleCounter);
				triangleIndices.Add(triangleCounter + 1);
				triangleIndices.Add(triangleCounter + 2);
				triangleIndices.Add(triangleCounter + 2);
				triangleIndices.Add(triangleCounter + 1);
				triangleIndices.Add(triangleCounter + 3);
			}
			((MeshGeometry3D)myBoxGeometryModel.Geometry).Positions = boxPoint3DCollection;
			((MeshGeometry3D)myBoxGeometryModel.Geometry).TriangleIndices = triangleIndices;
			_MyModel3DGroup.Children.Add(myBoxGeometryModel);
		}


		#region mouse interaction

		/**
		 * <summary>
		 * Method that zoom in and out on mouse wheel. Reference Code: https://www.codeproject.com/Articles/23332/WPF-D-Primer
		 * </summary>
		 *
		 * <param name="sender">sender object</param>
		 * <param name="e">arguments</param>
		 */
		private void _Viewport3DMouseWheel(object sender, MouseWheelEventArgs e)
		{
			_MainPerspectiveCamera.Position = new Point3D(
											_MainPerspectiveCamera.Position.X,
											_MainPerspectiveCamera.Position.Y,
											_MainPerspectiveCamera.Position.Z - e.Delta / 2D);
		}

		/**
		 * <summary>
		 * variable to control the viewport rotation through the mouse
		 * </summary>
		 */
		private bool _MouseDownFlag;

		/**
		 * <summary>
		 * variable to control the viewport rotation through the mouse
		 * </summary>
		 */
		private Point _MouseLastPos;

		/**
		 * <summary>
		 * Method to control the viewport rotation through the mouse. Reference Code: https://www.codeproject.com/Articles/23332/WPF-D-Primer
		 * </summary>
		 *
		 * <param name="sender">sender object</param>
		 * <param name="e">arguments</param>
		 */
		private void _Viewport3DMouseUp(object sender, MouseButtonEventArgs e)
		{
			_MouseDownFlag = false;
		}

		/**
		 * <summary>
		 * Method to control the viewport rotation through the mouse. Reference Code: https://www.codeproject.com/Articles/23332/WPF-D-Primer
		 * </summary>
		 *
		 * <param name="sender">sender object</param>
		 * <param name="e">arguments</param>
		 */
		private void _Viewport3DMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.LeftButton != MouseButtonState.Pressed) return;
			_MouseDownFlag = true;
			Point pos = Mouse.GetPosition(_MyViewport3D);
			_MouseLastPos = new Point(pos.X - _MyViewport3D.ActualWidth / 2, _MyViewport3D.ActualHeight / 2 - pos.Y);
		}

		/**
		 * <summary>
		 * Method to control the viewport rotation through the mouse. Reference Code: https://www.codeproject.com/Articles/23332/WPF-D-Primer
		 * </summary>
		 *
		 * <param name="sender">sender object</param>
		 * <param name="e">arguments</param>
		 */
		private void _Viewport3DMouseMove(object sender, MouseEventArgs e)
		{
			if (!_MouseDownFlag) return;
			Point pos = Mouse.GetPosition(_MyViewport3D);
			Point actualPos = new Point(pos.X - _MyViewport3D.ActualWidth / 2, _MyViewport3D.ActualHeight / 2 - pos.Y);
			double dx = actualPos.X - _MouseLastPos.X;
			double dy = actualPos.Y - _MouseLastPos.Y;
			double mouseAngle = 0;

			if (dx != 0 && dy != 0) {
				mouseAngle = Math.Asin(Math.Abs(dy) / Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2)));

				if (dx < 0 && dy > 0) mouseAngle += Math.PI / 2;
				else if (dx < 0 && dy < 0) mouseAngle += Math.PI;
				else if (dx > 0 && dy < 0) mouseAngle += Math.PI * 1.5;
			}
			else if (dx == 0 && dy != 0) {
				mouseAngle = Math.Sign(dy) > 0 ? Math.PI / 2 : Math.PI * 1.5;
			}
			else if (dx != 0 && dy == 0) {
				mouseAngle = Math.Sign(dx) > 0 ? 0 : Math.PI;
			}

			double axisAngle = mouseAngle + Math.PI / 2;

			Vector3D axis = new Vector3D(Math.Cos(axisAngle) * 4, Math.Sin(axisAngle) * 4, 0);

			double rotation = 0.02 * Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));

			Transform3DGroup group = _MyModel3DGroup.Transform as Transform3DGroup;

			if (group == null) {
				group = new Transform3DGroup();
				_MyModel3DGroup.Transform = group;
			}

			QuaternionRotation3D r =
				 new QuaternionRotation3D(
				 new Quaternion(axis, rotation * 180 / Math.PI));
			group.Children.Add(new RotateTransform3D(r));

			_MouseLastPos = actualPos;
		}

		#endregion mouse interaction
	}
}
