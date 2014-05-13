using System;

namespace Timeliner
{
	public sealed class Timer
	{
		public const float MinTimeStep = 0.0001f;
		public const float PPS = 50f; //pixels per second
		private float FCurrentTime;
		private float FHostTime;
		private float FSpeed = 1;
		private float FLastHostTime = -1;
		
		private bool FIsRunning;
		private bool FForceUpdate;
		
		public float LoopStart;
		public float LoopEnd;

		public float Time
		{
			get {return FCurrentTime;}
			set 
			{
				if (FCurrentTime != value)
				{
					FTimeDelta = value - FCurrentTime;
					FCurrentTime = value;
					FLastHostTime = FHostTime;
					FForceUpdate = true;
				}
			}
		}
		
		private float FTimeDelta;
		public float TimeDelta
		{
			get {return FTimeDelta;}
		}
		
		public float HostTime
		{
			get { return FHostTime;}
			set 
			{ 
				FHostTime = value;
				if (FLastHostTime == -1)
					FLastHostTime = value;
			}
		}
		
		public float Speed
		{
			get { return FSpeed;}
			set { FSpeed = value;}
		}
		
		public bool IsRunning
		{
			get { return FIsRunning;}
			set
			{
				FIsRunning = value;
			}
		}
		
		public override string ToString()
		{
            return TimeToString(FCurrentTime);
		}
        
        public string TimeToString(float time)
        {
            var showMinus = false;
			time = Math.Abs(time);
			
			if (FCurrentTime < 0)
				showMinus = true;
			
			var ms = (int) ((time - Math.Floor(time)) * 1000);
			var s = (int) (time % 60);
			var m = (int) (time / 60 % 60);
			var h = (int) (time / 60 / 60 % 60);
			//int d = (int) (time / 60 / 60 / 12 % 12);
			DateTime dt = new DateTime(2008, 1, 1, h, m, s, ms);

			if (showMinus)
				return "-" + dt.ToString("H:mm:ss:fff"); 
			else //add empty char for - placeholder
				return "\u00A0" + dt.ToString("H:mm:ss:fff");
        }
		
		public void Evaluate()
		{
			if (FIsRunning)
			{
				FTimeDelta = (FHostTime - FLastHostTime) * FSpeed;
				FCurrentTime += FTimeDelta;
				if (FCurrentTime > LoopEnd)
					FCurrentTime = LoopStart;
			}
			else if (FForceUpdate)
				FForceUpdate = false;
			else
				FTimeDelta = 0;
			
			FLastHostTime = FHostTime;
		}
        
        public void Play(bool run)
        {
            IsRunning = run;
        }
        
        public void Stop()
        {
            IsRunning = false;
            Time = 0;
        }
        
	}
}

