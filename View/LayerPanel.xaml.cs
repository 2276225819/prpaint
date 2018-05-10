using App2.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using LayerPaint;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace App2.View
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LayerPanel : Page
    {
        public LayerPanel()
        {
            this.InitializeComponent();
        }

        public ObservableCollection<LayerModel> Items { get; set; }



        public LayerModel CurrentLayer
        {
            get { return VModel.vm.CurrentLayer; }
            set { VModel.vm.CurrentLayer = value; }
        }
         


        private void OnNew(object sender, RoutedEventArgs e)
        { 
            var i = Items.IndexOf(CurrentLayer);
            LayerPaint.Exec.Do(new LayerPaint.Exec() {
                exec = () => {
                    var n = new LayerModel() { };
                    Items.Insert(i, n);
                    CurrentLayer = n;
                },
                undo = () => {
                    Items.RemoveAt(i);
                    CurrentLayer = Items[i];
                }
            });
        }

        private void OnDel(object sender, RoutedEventArgs e)
        {
            if (Items.Count == 1)
            {
                CurrentLayer.getRect(out Rect or, out WriteableBitmap ob);
                LayerPaint.Exec.Do(new LayerPaint.Exec() {
                    exec = () => {
                        CurrentLayer.setRect(default(Rect), null);
                    },
                    undo = () => {
                        CurrentLayer.setRect(or, ob);
                    }
                });
                return;
            }
            var o = CurrentLayer;
            var i = Items.IndexOf(o);
            var f = Items.Count == i + 1 ? 1 : 0;
            LayerPaint.Exec.Do(new LayerPaint.Exec() {
                exec = () => {
                    Items.RemoveAt(i);
                    CurrentLayer = Items[i - f];
                },
                undo = () => {
                    Items.Insert(i, o);
                    CurrentLayer = Items[i];
                }
            });
        }

        private void OnUp(object sender, RoutedEventArgs e)
        {
            var i = Items.IndexOf(CurrentLayer);
            if (i < 1)
            {
                return;
            }
            LayerPaint.Exec.Do(new LayerPaint.Exec() {
                exec = () => {
                    Items.Move(i, i - 1);  
                },
                undo = () => {
                    Items.Move(i - 1, i);  
                }
            });
        }

        private void OnDown(object sender, RoutedEventArgs e)
        {
            var i = Items.IndexOf(CurrentLayer);
            if (i >= Items.Count - 1)
            {
                return;
            }
            LayerPaint.Exec.Do(new LayerPaint.Exec() {
                exec = () => {
                    Items.Move(i, i + 1);  
                },
                undo = () => {
                    Items.Move(i + 1, i);  
                }
            });
        }

        private void OnCopy(object sender, RoutedEventArgs e)
        {
            var o = CurrentLayer;
            var n = new LayerModel() { Bitmap = o.Bitmap?.Clone(), X = o.X, Y = o.Y };
            var i = Items.IndexOf(o);
            LayerPaint.Exec.Do(new LayerPaint.Exec() {
                exec = () => { 
                    Items.Insert(i, n);
                    CurrentLayer = n;
                },
                undo = () => {
                    Items.RemoveAt(i);
                    CurrentLayer = Items[i];
                }
            });
        }
        private void OnExp(object sender, RoutedEventArgs e)
        {
            var i = Items.IndexOf(CurrentLayer);
            if (i + 1 >= Items.Count)
                return;
            var _u = Items[i];
            var _d = Items[i + 1];

            _u.getRect(out Rect ur, out WriteableBitmap ub);
            _d.getRect(out Rect dr, out WriteableBitmap db);
            var exm = RectHelper.Union(ur, dr);
            var ex = new WriteableBitmap((int)exm.Width, (int)exm.Height);
            IGrap.addImg(db, ex, (int)(_d.X - exm.X), (int)(_d.Y - exm.Y), _d.Opacity);
            IGrap.addImg(ub, ex, (int)(_u.X - exm.X), (int)(_u.Y - exm.Y), _u.Opacity);

            Exec.Do(new Exec() {
                exec = () => {
                    Items.RemoveAt(i + 1);
                    Items[i].setRect(exm, ex);
                },
                undo = () => {
                    Items[i].setRect(ur, ub);
                    Items.Insert(i + 1, new LayerModel {
                        Bitmap = db,
                        X = dr.X,
                        Y = dr.Y,
                    });
                },
            });
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SLIDER.DataContext = CurrentLayer;
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }
    }
}
