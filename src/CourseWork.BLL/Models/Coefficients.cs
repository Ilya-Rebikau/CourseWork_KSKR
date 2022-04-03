namespace CourseWork.BLL.Models
{
    public static class Coefficients
    {
        private static double _poissonRatio;

        /// <summary>
        /// Коэффициент Пуассона.
        /// </summary>
        public static double PoissonRatio
        {
            get
            {
                return _poissonRatio;
            }
            set
            {
                if (value > 0 && value < 0.5)
                {
                    _poissonRatio = value;
                }
                else
                {
                    throw new ArgumentException("Коэффициент Пуассона должен быть больше 0 и меньше 0.5!");
                }
            }
        }

        /// <summary>
        /// Толщина элемента.
        /// </summary>
        public static double Thickness { get; set; }

        /// <summary>
        /// Модуль Юнга.
        /// </summary>
        public static double YoungModule { get; set; }
    }
}
