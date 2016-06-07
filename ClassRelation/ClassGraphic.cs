using ClassRelation;
using Mono.Cecil;
using Mono.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ClassTool
{
    public class ClassGraphic
    {
        private int _width;
        private int _height;
        private int _classWidth = 150;
        private int _classHeight = 30;

        private int _top = 20;
        private int _left = 20;
        private int _bottom = 20;
        private int _ritht = 20;
        private int _rowSpan = 50;
        private int _colSpan = 50;
        private int _fontSize = 12;

        private int _xMaxCount;
        private int _yMaxCount;

        private TypeInfo _currentType;
        private Graphics _g;
        private Bitmap image;

        private GraphicElementManager _graphicElementDic;

        public ClassGraphic(int xMaxCount, int yMaxCount):this(xMaxCount,yMaxCount,null)
        {

        }
        public ClassGraphic(int xMaxCount, int yMaxCount,Graphics g)
        {
            this._graphicElementDic = new GraphicElementManager();
            this._xMaxCount = xMaxCount < 1 ? 1 : xMaxCount;
            this._yMaxCount = yMaxCount < 1 ? 1 : yMaxCount;

            this._width = _left + _ritht + _classWidth * _xMaxCount + _colSpan * (_xMaxCount - 1);
            this._height = _top + _bottom + _classHeight * _yMaxCount + _rowSpan * (_yMaxCount - 1);
            if (g != null)
            {
                this._g = g;
            }
            else
            {
                image = new Bitmap(_width, _height);
                _g = Graphics.FromImage(image);
            }
            _g.Clear(Color.White);

        }

        public Bitmap Draw(ClassInfo classInfo)
        {
            this._currentType = classInfo;
            var startEl = DrawSelfAndBase(classInfo, 1);
            int startX = startEl.X + this._classWidth / 2;
            int startY = startEl.Y +this._classHeight;
            Pen pen = new Pen(Color.Blue, 2);
            foreach (var subInfo in classInfo.SubType)
            {
                var subEl = DrawSub(subInfo, classInfo.SubType.Count);
                int endX = subEl.X + _classWidth / 2;
                int endY = subEl.Y;
                _g.DrawLine(pen, startX, startY, endX, endY);

            }
            //image.RotateFlip(RotateFlipType.Rotate180FlipX);
            return image;
        }

        //draw slef and all baseTypes
        protected virtual GraphicElement DrawSelfAndBase(TypeInfo info, int totalInSameDepth)
        {
            if (info == null)
                return null;
            var typeEl = _graphicElementDic.GetElement(info);
            if (typeEl == null)
            {
                typeEl = CreateElement(info, totalInSameDepth);
                int x = typeEl.X;
                int y = typeEl.Y;
                SolidBrush brush = new SolidBrush(Color.Black);                             
                Pen pen = new Pen(Color.Blue, 2);               
                _g.DrawRectangle(pen, x, y, _classWidth, _classHeight);
                Brush fillBrush;
                if (this._currentType == info)
                {
                    fillBrush = new SolidBrush(Color.LightGreen);
                }
                else {
                    if (info is ClassInfo)
                    {
                        fillBrush = new SolidBrush(Color.LightSkyBlue);
                    }
                    else
                    {
                        fillBrush = new SolidBrush(Color.LightYellow);
                    }
                }               

                _g.FillRectangle(fillBrush,x, y, _classWidth, _classHeight);

                Font font = new Font("微软雅黑", _fontSize);

                int textX = x ;
                int textY = y + (_classHeight - font.Height) / 2;
                _g.DrawString(info.Name, font, brush, textX, textY);

                int eleCount = info.BaseCountInSameDepth;
                int startX = x + _classWidth / 2;
                int startY = y;

                if (info is ClassInfo)
                {
                    var classInfo = info as ClassInfo;
                    if (classInfo.BaseClassInfo != null)
                    {
                        var baseEl = DrawSelfAndBase(classInfo.BaseClassInfo, eleCount);
                        int endX = baseEl.X + _classWidth / 2;
                        int endY = baseEl.Y + _classHeight;
                        _g.DrawLine(pen, startX, startY, endX, endY);
                    }

                }                
                pen.Color = Color.LawnGreen;
                foreach (var baseInfo in info.BaseInterfaceInfos)
                {
                    var baseEl = DrawSelfAndBase(baseInfo, eleCount);
                    int endX = baseEl.X + _classWidth / 2;
                    int endY = baseEl.Y + this._classHeight;
                    _g.DrawLine(pen, startX, startY, endX, endY);

                }
            }
            return typeEl;
        }

        //draw all child Types
        protected virtual GraphicElement DrawSub(TypeInfo info, int totalInSameDepth)
        {
            if (info == null)
                return null;
            var typeEl = _graphicElementDic.GetElement(info);
            if (typeEl == null)
            {
                typeEl = CreateElement(info, totalInSameDepth);
                int x = typeEl.X;
                int y = typeEl.Y;
                SolidBrush brush = new SolidBrush(Color.Black);
                Pen pen = new Pen(Color.Blue, 2);
                _g.DrawRectangle(pen, x, y, _classWidth, _classHeight);
                Brush fillBrush;
                if (info is ClassInfo)
                {
                    fillBrush = new SolidBrush(Color.LightSkyBlue);
                }
                else
                {
                    fillBrush = new SolidBrush(Color.LightYellow);
                }

                _g.FillRectangle(fillBrush, x, y, _classWidth, _classHeight);

                Font font = new Font("微软雅黑", _fontSize);

                int textX = x;
                int textY = y + (_classHeight - font.Height) / 2;
                _g.DrawString(info.Name, font, brush, textX, textY);

                int startX = x + _classWidth / 2;
                int startY = y + _classHeight;
                if (info.SubType != null)
                {
                    int eleCount = info.SubType.Count;
                    foreach (var subInfo in info.SubType)
                    {
                        var baseEl = DrawSub(subInfo, eleCount);
                        int endX = baseEl.X + _classWidth / 2;
                        int endY = baseEl.Y;
                        _g.DrawLine(pen, startX, startY, endX, endY);

                    }
                }
            }
            return typeEl;
        }

        private GraphicElement CreateElement(TypeInfo type, int totalInSameDepth)
        {
            if (_graphicElementDic.GetElement(type) != null)
                throw new InvalidOperationException("已经创建过了");
            GraphicElement el = new GraphicElement(this._yMaxCount);
            el.Info = type;
            el.Index = _graphicElementDic.GetCountInSameDepth(el.Depth) + 1;
            int eleWidth = _classWidth * totalInSameDepth + _colSpan * (totalInSameDepth - 1);
            int offsetX = (this._width - eleWidth-_left-_ritht) / 2;
            el.X = _left + offsetX + (el.Index - 1) * (_classWidth + _colSpan);
            el.Y = _top + (el.Depth - 1) * (_classHeight + _rowSpan);
            _graphicElementDic.Add(el.Depth, el);
            return el;
        }

    }
}
