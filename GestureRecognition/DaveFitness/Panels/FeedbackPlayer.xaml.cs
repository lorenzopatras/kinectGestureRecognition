﻿using GestureRecognition;
using SkeletonModel.Model;
using SkeletonModel.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DaveFitness.Panels {
  public partial class FeedbackPlayer : UserControl {
    public FeedbackPlayer() {
      InitializeComponent();
      Visibility = System.Windows.Visibility.Hidden;
    }

    public void PlayFeedback(GestureManager gestureManager) {//Body[] record, Body[] reference) {
      Visibility = System.Windows.Visibility.Visible;
      
      this.record = gestureManager.GestureDetector.RecordSample;
      this.reference = gestureManager.GestureDetector.ReferenceSample;
      this.gestureManager = gestureManager;

      StartPlayback();
    }

    private GestureManager gestureManager;

    private void StartPlayback() {
      gestureManager.PauseRecognition();

      referenceIndex = 0;
      recordIndex = 0;
      timer = new System.Timers.Timer { Interval = 40 };
      timer.Elapsed += UpdateFeedbackPanels;
      timer.Start();
    }

    private void UpdateFeedbackPanels(object sender, ElapsedEventArgs e) {
      if (recordIndex == record.Length && referenceIndex == reference.Length) {
        recordIndex = 0;
        referenceIndex = 0;
        timer.Elapsed -= UpdateFeedbackPanels;
        timer.Stop();
        gestureManager.ResumeRecognition();
        
        this.Dispatcher.Invoke((Action)(() => { // update from any thread
          Visibility = System.Windows.Visibility.Hidden;
        }));

        return;
      }

      // play last sample until both are at the end
      if (recordIndex == record.Length) {
        recordIndex--;
      }

      if (referenceIndex == reference.Length) {
        referenceIndex--;
      }

      if (record != null && reference != null) {
        Body recSample = record[recordIndex++];
        Body refSample = reference[referenceIndex++];

        this.Dispatcher.Invoke((Action)(() => { // update from any thread
          PlotParallelFeedback(refSample, recSample);
          PlotRecordFeedback(recSample);
          PlotReferenceFeedback(refSample);
        }));
      }

     
    }

    private void PlotReferenceFeedback(Body reference) {
      referenceCanvas.Children.Clear();

      centerX = (int)referenceCanvas.Width / 2;
      centerY = (int)referenceCanvas.Height / 2;

      SkeletonModel.Model.Joint centerReferenceJoint = reference.JointSkeleton.GetJoint(JointName.HipCenter);

      DrawBones(reference, centerReferenceJoint, referenceCanvas, Colors.Black);
      DrawJoints(reference, centerReferenceJoint, referenceCanvas, Colors.Red);
    }

    private void PlotRecordFeedback(Body record) {
      recordCanvas.Children.Clear();

      centerX = (int)recordCanvas.Width / 2;
      centerY = (int)recordCanvas.Height / 2;

      SkeletonModel.Model.Joint centerRecordJoint = record.JointSkeleton.GetJoint(JointName.HipCenter);

      DrawBones(record, centerRecordJoint, recordCanvas, Colors.Black);
      DrawJoints(record, centerRecordJoint, recordCanvas, Colors.Red);
    }

    private void DrawJoints(Body body, SkeletonModel.Model.Joint centerJoint, Canvas canvas, Color color) {
      foreach (JointName jointType in Enum.GetValues(typeof(JointName))) {
        SkeletonModel.Model.Joint joint = body.JointSkeleton.GetJoint(jointType);

        if (joint == null) continue;

        double x = joint.XCoord - centerJoint.XCoord;
        double y = joint.YCoord - centerJoint.YCoord;

        DrawPoint(centerX + x * 150, centerY - y * 150, color, canvas);
      }
    }

    private void DrawBones(Body body, SkeletonModel.Model.Joint centerJoint, Canvas canvas, Color color) {
      foreach (BoneName boneName in Enum.GetValues(typeof(BoneName))) {
        Tuple<JointName, JointName> boneExtremities = Mapper.BoneJointMap[boneName];
        SkeletonModel.Model.Joint startJoint = body.JointSkeleton.GetJoint(boneExtremities.Item1);
        SkeletonModel.Model.Joint endJoint = body.JointSkeleton.GetJoint(boneExtremities.Item2);

        if (startJoint == null || endJoint == null) continue;

        double x1 = startJoint.XCoord - centerJoint.XCoord;
        double y1 = startJoint.YCoord - centerJoint.YCoord;
        double x2 = endJoint.XCoord - centerJoint.XCoord;
        double y2 = endJoint.YCoord - centerJoint.YCoord;

        DrawLine(centerX + x1 * 150, centerY - y1 * 150, centerX + x2 * 150, centerY - y2 * 150, color, canvas);
      }
    }



    private void PlotParallelFeedback(Body reference, Body record) {
      parallelCanvas.Children.Clear();

      centerX = (int)parallelCanvas.Width / 2;
      centerY = (int)parallelCanvas.Height / 2;

      SkeletonModel.Model.Joint centerReferenceJoint = reference.JointSkeleton.GetJoint(JointName.HipCenter);
      SkeletonModel.Model.Joint centerRecordJoint = record.JointSkeleton.GetJoint(JointName.HipCenter);

      //DrawPoint(centerX, centerY, Colors.Yellow, parallelCanvas);
      //DrawPoint(centerX, centerY, Colors.Red, parallelCanvas);

      foreach (JointName jointType in Enum.GetValues(typeof(JointName))) {
        SkeletonModel.Model.Joint joint = reference.JointSkeleton.GetJoint(jointType);

        if (joint == null) continue;

        double x = joint.XCoord - centerReferenceJoint.XCoord;
        double y = joint.YCoord - centerReferenceJoint.YCoord;

        DrawPoint(centerX + x * 150, centerY - y * 150, Colors.Yellow, parallelCanvas);
      }

      foreach (BoneName boneName in Enum.GetValues(typeof(BoneName))) {
        Tuple<JointName, JointName> boneExtremities = Mapper.BoneJointMap[boneName];
        SkeletonModel.Model.Joint startJoint = reference.JointSkeleton.GetJoint(boneExtremities.Item1);
        SkeletonModel.Model.Joint endJoint = reference.JointSkeleton.GetJoint(boneExtremities.Item2);

        if (startJoint == null || endJoint == null) continue;

        double x1 = startJoint.XCoord - centerReferenceJoint.XCoord;
        double y1 = startJoint.YCoord - centerReferenceJoint.YCoord;
        double x2 = endJoint.XCoord - centerReferenceJoint.XCoord;
        double y2 = endJoint.YCoord - centerReferenceJoint.YCoord;

        DrawLine(centerX + x1 * 150, centerY - y1 * 150, centerX + x2 * 150, centerY - y2 * 150, Colors.OrangeRed, parallelCanvas);
      }

      foreach (JointName jointType in Enum.GetValues(typeof(JointName))) {
        SkeletonModel.Model.Joint joint = record.JointSkeleton.GetJoint(jointType);

        if (joint == null) continue;

        double x = joint.XCoord - centerRecordJoint.XCoord;
        double y = joint.YCoord - centerRecordJoint.YCoord;

        DrawPoint(centerX + x * 150, centerY - y * 150, Colors.Red, parallelCanvas);
      }

      foreach (BoneName boneName in Enum.GetValues(typeof(BoneName))) {
        Tuple<JointName, JointName> boneExtremities = Mapper.BoneJointMap[boneName];
        SkeletonModel.Model.Joint startJoint = record.JointSkeleton.GetJoint(boneExtremities.Item1);
        SkeletonModel.Model.Joint endJoint = record.JointSkeleton.GetJoint(boneExtremities.Item2);

        if (startJoint == null || endJoint == null) continue;

        double x1 = startJoint.XCoord - centerRecordJoint.XCoord;
        double y1 = startJoint.YCoord - centerRecordJoint.YCoord;
        double x2 = endJoint.XCoord - centerRecordJoint.XCoord;
        double y2 = endJoint.YCoord - centerRecordJoint.YCoord;

        DrawLine(centerX + x1 * 150, centerY - y1 * 150, centerX + x2 * 150, centerY - y2 * 150, Colors.Black, parallelCanvas);
      }
    }

    private void DrawPoint(double x, double y, Color color, Canvas canvas) {
      Ellipse point = new Ellipse {
        Width = 7,
        Height = 7,
        Fill = new SolidColorBrush(color)
      };

      Canvas.SetLeft(point, x - point.Width / 2);
      Canvas.SetTop(point, y - point.Height / 2);
      canvas.Children.Add(point);
    }

    private void DrawLine(double x1, double y1, double x2, double y2, Color color, Canvas canvas) {
      Line myLine = new Line();
      myLine.Stroke = new SolidColorBrush(color);
      myLine.X1 = x1;
      myLine.X2 = x2;
      myLine.Y1 = y1;
      myLine.Y2 = y2;
      myLine.HorizontalAlignment = HorizontalAlignment.Left;
      myLine.VerticalAlignment = VerticalAlignment.Top;
      myLine.StrokeThickness = 5;
      canvas.Children.Add(myLine);
    }

    private int recordIndex = 0;
    private int referenceIndex = 0;
    private Timer timer;
    private Body[] record;
    private Body[] reference;
    private int centerX;
    private int centerY;
  }
}
