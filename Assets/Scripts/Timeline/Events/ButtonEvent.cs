using System;

namespace GMTK
{
	//stored on the heap!
	public class ButtonEvent : IEquatable<ButtonEvent>
	{
		public Buttons Button;
		public long PressFrame = -1;
		public long ReleaseFrame = -1;

		public bool IsPressed(long frame)
		{
			if (PressFrame == -1)
			{
				return false;
			}
			
			if (ReleaseFrame < 0)
			{
				return frame > PressFrame;	
			}
			
			return frame >= PressFrame && frame < ReleaseFrame;
		}

		public bool IsFirstPressed(long frame)
		{
			return PressFrame == frame;
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
			return HashCode.Combine((int)Button, PressFrame, ReleaseFrame);
		}
	}
}