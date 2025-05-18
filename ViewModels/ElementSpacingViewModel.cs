using DocCreator01.Models;
using ReactiveUI;

namespace DocCreator01.ViewModels
{
    public class ElementSpacingViewModel : ReactiveObject
    {
        private double _top;
        public double Top
        {
            get => _top;
            set => this.RaiseAndSetIfChanged(ref _top, value);
        }

        private double _right;
        public double Right
        {
            get => _right;
            set => this.RaiseAndSetIfChanged(ref _right, value);
        }

        private double _bottom;
        public double Bottom
        {
            get => _bottom;
            set => this.RaiseAndSetIfChanged(ref _bottom, value);
        }

        private double _left;
        public double Left
        {
            get => _left;
            set => this.RaiseAndSetIfChanged(ref _left, value);
        }

        public ElementSpacingInfo ToDto()
        {
            return new ElementSpacingInfo
            {
                Top = this.Top,
                Right = this.Right,
                Bottom = this.Bottom,
                Left = this.Left
            };
        }

        public void LoadFromDto(ElementSpacingInfo dto)
        {
            Top = dto.Top;
            Right = dto.Right;
            Bottom = dto.Bottom;
            Left = dto.Left;
        }
    }
}
