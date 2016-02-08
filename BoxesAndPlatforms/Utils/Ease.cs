using System;

namespace BoxesAndPlatforms {
	// Static class with functions used to make game feel "smoother"
	// Combined with Utils.lerp they can be used to interpolate from one point to another with a specific mathematical function

	static public class Ease {
		public static float linear(float time) {
			return Utils.clamp(time, 0, 1);
		}

		public static float exp(float value, float dest, float perc) {
			return Utils.lerp(value, dest, 1 - perc);
		}

		public static float exp(float value, float dest, float perc, float time) {
			return Utils.lerp(value, dest, 1 - MathF.Pow(perc, time));
		}

		public static float expAngle(float value, float dest, float perc) {
			return Utils.lerpAngle(value, dest, 1 - perc);
		}

		public static float expAngle(float value, float dest, float perc, float time) {
			return Utils.lerpAngle(value, dest, 1 - MathF.Pow(perc, time));
		}

		public static float powIn(float time, float pow) {
			return MathF.Pow(Utils.clamp(time, 0, 1), pow);
		}

		public static float powOut(float time, float pow) {
			return 1 - MathF.Pow(1 - Utils.clamp(time, 0, 1), pow);
		}

		public static float powInOut(float time, float pow) {
			time = 2 * Utils.clamp(time, 0, 1);

			if (time < 1)
				return 0.5f * MathF.Pow(time, pow);

			return 1 - 0.5f * Math.Abs(MathF.Pow(2 - time, pow));
		}

		public static float quadIn(float time) {
			return powIn(time, 2);
		}

		public static float quadOut(float time) {
			return powOut(time, 2);
		}

		public static float quadInOut(float time) {
			return powInOut(time, 2);
		}

		public static float cubicIn(float time) {
			return powIn(time, 3);
		}

		public static float cubicOut(float time) {
			return powOut(time, 3);
		}

		public static float cubicInOut(float time) {
			return powInOut(time, 3);
		}

		public static float quartIn(float time) {
			return powIn(time, 4);
		}

		public static float quartOut(float time) {
			return powOut(time, 4);
		}

		public static float quartInOut(float time) {
			return powInOut(time, 4);
		}

		public static float quintIn(float time) {
			return powIn(time, 5);
		}

		public static float quintOut(float time) {
			return powOut(time, 5);
		}

		public static float quintInOut(float time) {
			return powInOut(time, 5);
		}

		public static float sineIn(float time) {
			return 1 - MathF.Cos(Utils.clamp(time, 0, 1) * MathF.PI / 2);
		}

		public static float sineOut(float time) {
			return MathF.Sin(Utils.clamp(time, 0, 1) * MathF.PI / 2);
		}

		public static float sineInOut(float time) {
			return -0.5f * (MathF.Cos(MathF.PI * Utils.clamp(time, 0, 1)) - 1);
		}

		public static float circIn(float time) {
			return -(MathF.Sqrt(1 - Utils.clamp(time, 0, 1) * Utils.clamp(time, 0, 1)) - 1);
		}

		public static float circOut(float time) {
			time = Utils.clamp(time, 0, 1);
			return MathF.Sqrt(1 - time * (time - 1));
		}

		public static float circInOut(float time) {
			time = 2 * Utils.clamp(time, 0, 1);

			if (time < 1)
				return -0.5f * (MathF.Sqrt(1 - time * time) - 1);

			return 0.5f * (MathF.Sqrt(1 - (time - 2) * (time - 2)) + 1);
		}

		public static float bounceIn(float time) {
			return 1 - Ease.bounceOut(1 - time);
		}

		public static float bounceOut(float time) {
			if (time < 0)
				return 0;

			if (time > 1)
				return 1;

			if (time < 1 / 2.75f)
				return 7.5625f * time * time;
			else if (time < 2 / 2.75f)
				return 7.5625f * (time - 1.5f / 2.75f) * (time - 1.5f / 2.75f) + 0.75f;
			else if (time < 2.5f / 2.75f)
				return 7.5625f * (time - 2.25f / 2.75f) * (time - 2.25f / 2.75f) + 0.9375f;
			else
				return 7.5625f * (time - 2.625f / 2.75f) * (time - 2.625f / 2.75f) + 0.984375f;
		}

		public static float bounceInOut(float time) {
			if (time < 0.5f)
				return Ease.bounceIn(time * 2) * 0.5f;

			return Ease.bounceOut(time * 2 - 1) * 0.5f + 0.5f;
		}

		public static float elasticIn(float time, float amplitude = 1, float period = 0.3f) {
			if (time < 0)
				return 0;
			
			if (time > 1)
				return 1;

			var s = period / MathF.PI / 2 * MathF.Asin(1 / amplitude);

			return -(amplitude * MathF.Pow(2, 10 * (time - 1)) * MathF.Sin((time - s - 1) * MathF.PI * 2 / period));
		}

		public static float elasticOut(float time, float amplitude = 1, float period = 0.3f) {
			if (time < 0)
				return 0;

			if (time > 1)
				return 1;

			var s = period / MathF.PI / 2 * MathF.Asin(1 / amplitude);

			return (amplitude * MathF.Pow(2, -10 * time) * MathF.Sin((time - s) * MathF.PI * 2 / period) + 1);
		}

		public static float elasticInOut(float time, float amplitude = 1, float period = 0.45f) {
			if (time < 0)
				return 0;

			if (time > 1)
				return 1;

			time *= 2;

			var s = period / MathF.PI / 2 * MathF.Asin(1 / amplitude);

			if (time < 1)
				return -0.5f * amplitude * MathF.Pow(2, 10 * (time - 1)) * MathF.Sin((time - s - 1) * MathF.PI * 2 / period);

			return 0.5f * amplitude * MathF.Pow(2, -10 * (time - 1)) * MathF.Sin((time - s) * MathF.PI * 2 / period) + 1;
		}
	}
}
