using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Peppa
{
    public class ChatMessage
    {
        private ImageSource _headImage;
        public ImageSource HeadImage
        {
            get { return _headImage; }
            set { _headImage = value; }
        }

        private string _message;
        /// <summary>
        /// 显示的内容
        /// </summary>
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }

        private FlowDirection _flowDir;
        /// <summary>
        /// 显示的方向
        /// </summary>
        public FlowDirection FlowDir
        {
            get { return _flowDir; }
            set { _flowDir = value; }
        }

        private Brush _backColor;
        /// <summary>
        /// 显示的背景颜色
        /// </summary>
        public Brush BackColor
        {
            get { return _backColor; }
            set { _backColor = value; }
        }

        public UIElement Content { get; set; }

        public int Type { get; set; }

    }
}
