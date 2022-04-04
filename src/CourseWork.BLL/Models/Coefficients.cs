namespace CourseWork.BLL.Models
{
    public static class Coefficients
    {
        private static double _poissonRatio;

        private static double _thickness;

        private static double _youngModule;

        private static int _meshStep;

        private static double _force;

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
        public static double Thickness
        {
            get
            {
                return _thickness;
            }
            set
            {
                if (value > 0 && value < double.MaxValue)
                {
                    _thickness = value;
                }
                else
                {
                    throw new ArgumentException($"Толщина должна быть больше 0 и меньше {double.MaxValue}");
                }
            }
        }

        /// <summary>
        /// Модуль Юнга.
        /// </summary>
        public static double YoungModule
        {
            get
            {
                return _youngModule;
            }
            set
            {
                if (value > 0 && value < double.MaxValue)
                {
                    _youngModule = value;
                }
                else
                {
                    throw new ArgumentException($"Модуль Юнга должен быть больше 0 и меньше {double.MaxValue}");
                }
            }
        }

        /// <summary>
        /// Шаг сетки.
        /// </summary>
        public static int MeshStep
        {
            get
            {
                return _meshStep;
            }
            set
            {
                if (value > 0 && value < int.MaxValue)
                {
                    _meshStep = value;
                }
                else
                {
                    throw new ArgumentException($"Неверный шаг сетки");
                }
            }
        }

        /// <summary>
        /// Сила, приложенная к конечному элементу.
        /// </summary>
        public static double Force
        {
            get
            {
                return _force;
            }
            set
            {
                if (value > 0 && value < double.MaxValue)
                {
                    _force = value;
                }
                else
                {
                    throw new ArgumentException($"Прилагаемая сила должна быть больше 0 и меньше {double.MaxValue}");
                }
            }
        }
    }
}
