namespace CG
{
    public class Collision
    {
        public Disk Object1, Object2;
        public double Time;

        public Collision(Disk object1, Disk object2, double time)
        {
            Object1 = object1;
            Object2 = object2;
            Time = time;
        }

        public override string ToString()
        {
            return $"{nameof(Time)}: {Time:0.00}, {nameof(Object1)}: {Object1}, {nameof(Object2)}: {Object2}";
        }
    }
}
