using System;
using System.Security.Cryptography;

namespace terrainGeneratorSample
{
	/**
	 * <summary>
	 * Class that automatically generate realistic terrain
	 * </summary>
	 */
	public class TerrainGenerator
	{
		#region public method and properties

		/**
		 * <summary>
		 * Property to read the size of the generated map
		 * </summary>
		 */
		public int Size
		{
			get { return _Size; }
		}

		/**
		 * <summary>
		 * Property to retrieve the generated map
		 * </summary>
		 */
		public float[,] Map
		{
			get { return _Map; }
		}

		/**
		 * <summary>
		 * Constructor of the class
		 * </summary>
		 *
		 * <param name="detail">detail of the map to generate.</param>
		 * <remarks>the size of the map will be (2^detail - 1)</remarks>
		 */
		public TerrainGenerator(int detail)
		{
			_Size = (int)(Math.Pow(2, detail) + 1);
			_Max = _Size - 1;
			_Map = new float[_Size,_Size];
		}
		
		/**
		 * <summary>
		 * Method that generate the terrain
		 * </summary>
		 *
		 * <param name="roughness">Surface 'roughness' of the map. Desiderable value are between 0.2 and 0.8</param>
		 */
		public void Generate(float roughness)
		{
			_Roughness = roughness;
			_SetMapValue(0, 0, (float)_Max / 2);
			_SetMapValue(_Max, 0, 0);
			_SetMapValue(_Max, _Max, (float)_Max / 2);
			_SetMapValue(0, _Max, _Max);
			_Divide(_Max);
		}
		#endregion public method

		#region private variable and methods

		/**
		 * <summary>
		 * Map size
		 * </summary>
		 */
		private int _Size;
		/**
		 * <summary>
		 * Max map limiter
		 * </summary>
		 */
		private int _Max;
		/**
		 * <summary>
		 * Generated map
		 * </summary>
		 */
		private float[,] _Map;

		/**
		 * <summary>
		 * Surface roughness
		 * </summary>
		 */
		private float _Roughness;

		/**
		 * <summary>
		 * Random number generator
		 * </summary>
		 */
		Random _Random = new Random();

		/**
		 * <summary>
		 * Recursive divide the map in smaller region to generate height values
		 * </summary>
		 *
		 * <param name="size">Size of the region</param>
		 */
		public void _Divide(int size)
		{
			var half = size / 2;
			var scale = _Roughness * size;
			if (half < 1) return;
			
			for (int y = half; y < _Max; y += size) {
				for (int x = half; x < _Max; x += size) {
					_Square(x, y, half, (float)(_Random.NextDouble() * scale * 2 - scale));
				}
			}
			for (int y = 0; y <= _Max; y += half) {
				for (int x = (y + half) % size; x <= _Max; x += size) {
					_Diamond(x, y, half, (float)(_Random.NextDouble() * scale * 2 - scale));
				}
			}
			_Divide(size / 2);
		}

		/**
		 * <summary>
		 * Square phase averages four corner points before applying a random offset
		 * </summary>
		 *
		 * <param name="x">x position of the region</param>
		 * <param name="y">y position of the region</param>
		 * <param name="size">size of the region</param>
		 * <param name="offset">random offset</param>
		 */
		private void _Square(int x, int y, int size, float offset)
		{
			float[] data = { _GetMapValue(x - size, y - size), _GetMapValue(x + size, y - size), _GetMapValue(x + size, y + size), _GetMapValue(x - size, y + size) };
			float ave = _Average(data);
			_SetMapValue(x, y, ave + offset);
		}
		/**
		 * <summary>
		 * Diamond phase averages four edge points before applying a random offset
		 * </summary>
		 *
		 * <param name="x">x position of the region</param>
		 * <param name="y">y position of the region</param>
		 * <param name="size">size of the region</param>
		 * <param name="offset">random offset</param>
		 */
		private void _Diamond(int x, int y, int size, float offset)
		{
			float[] data = { _GetMapValue(x, y - size), _GetMapValue(x + size, y), _GetMapValue(x, y + size), _GetMapValue(x - size, y) };
			float ave = _Average(data);
			_SetMapValue(x, y, ave + offset);
		}

		/**
		 * <summary>
		 * Method that compute the average of the imput values. It discard not consistent values (-1).
		 * </summary>
		 *
		 * <param name="values">array of values to average</param>
		 * <returns>The average value of the input array</returns>
		 */
		private float _Average(float[] values)
		{
			float total = 0;
			float validLenght = 0;
			foreach (int dat in values) {
				if (dat != -1) {
					total += dat;
					validLenght++;
				}
			}

			return total / validLenght;
		}

		/**
		 * <summary>
		 * Method to get a value from the map
		 * </summary>
		 *
		 * <param name="x">map x position</param>
		 * <param name="y">map y position</param>
		 * <returns>the value of the coordinates if valid, -1 otherwise</returns>
		 */
		private float _GetMapValue(int x, int y)
		{
			if (x < 0 || x > this._Max || y < 0 || y > this._Max) return -1;
			return _Map[x, y];
		}

		/**
		 * <summary>
		 * Method to set a value in the map
		 * </summary>
		 *
		 * <param name="x">map x position</param>
		 * <param name="y">map y position</param>
		 * <param name="value">value to set</param>
		 */
		private void _SetMapValue(int x, int y, float value)
		{
			_Map[x,y] = value;
		}

		#endregion
	}
}
