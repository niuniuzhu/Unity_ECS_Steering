using Unity.Mathematics;

namespace Steering
{
	public interface IRandom
	{
		float2 onUnitCircle { get; }

		float2 insideUnitCircle { get; }

		float NextFloat( float min, float max );

		float NextFloat();

		int NextInt( int min = int.MinValue, int max = int.MaxValue );
	}
}