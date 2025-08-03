using System;
using UnityEngine;

namespace GMTK
{
	//stored on the heap!
	public class ButtonEvent : IEquatable<ButtonEvent>
	{
		public const Buttons AnyDir = (Buttons.Down | Buttons.Left | Buttons.Up | Buttons.Right);

		public Buttons Button = Buttons.None;
		public long PressFrame = -1;
		public long ReleaseFrame = -1;
		private readonly uint _guid = GetID();

		private static uint _id;
		private static uint GetID()
		{
			_id++;
			return _id;
		}

		public bool IsPressed(long frame)
		{
			if (PressFrame == -1)
			{
				return false;
			}
			
			if (ReleaseFrame < 0)
			{
				return frame >= PressFrame;	
			}
			
			return frame >= PressFrame && frame < ReleaseFrame;
		}

		public bool IsFirstPressed(long frame)
		{
			return PressFrame == frame;
		}

		public Vector2 GetDir()
		{
			if ((Button & AnyDir) == 0)
			{
				return Vector2.zero;
			}
			switch (Button)
			{
				case Buttons.Up:
					return Vector2.up;
				case Buttons.Down:
					return Vector2.down;
				case Buttons.Left:
					return Vector2.left;
				case Buttons.Right:
					return Vector2.right;
				
				case Buttons.UpLeft:
					return new Vector2(-1, 1);
				case Buttons.UpRight:
					return new Vector2(1, 1);
				
				case Buttons.DownLeft:
					return new Vector2(-1, -1);
				case Buttons.DownRight:
					return new Vector2(1, -1);
				
				default:
					return Vector2.zero;
			}
		}

		public override string ToString()
		{
			return $"{Button} p:{PressFrame} r:{ReleaseFrame} l:{ReleaseFrame-PressFrame}";
		}

		public bool Equals(ButtonEvent other)
		{
			if (other is null) return false;
			if (ReferenceEquals(this, other)) return true;
			return Button == other.Button && PressFrame == other.PressFrame && ReleaseFrame == other.ReleaseFrame;
		}

		public override bool Equals(object obj)
		{
			if (obj is null) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;
			return Equals((ButtonEvent)obj);
		}

		public override int GetHashCode()
		{
			return _guid.GetHashCode();
		}
	}
}