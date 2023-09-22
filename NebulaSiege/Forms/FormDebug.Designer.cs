﻿namespace NebulaSiege.Forms
{
    partial class FormDebug
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormDebug));
            splitContainerBody = new System.Windows.Forms.SplitContainer();
            textBoxOutput = new System.Windows.Forms.TextBox();
            buttonExecute = new System.Windows.Forms.Button();
            comboBoxInput = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)splitContainerBody).BeginInit();
            splitContainerBody.Panel1.SuspendLayout();
            splitContainerBody.Panel2.SuspendLayout();
            splitContainerBody.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainerBody
            // 
            splitContainerBody.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainerBody.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            splitContainerBody.IsSplitterFixed = true;
            splitContainerBody.Location = new System.Drawing.Point(0, 0);
            splitContainerBody.Name = "splitContainerBody";
            splitContainerBody.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerBody.Panel1
            // 
            splitContainerBody.Panel1.Controls.Add(textBoxOutput);
            // 
            // splitContainerBody.Panel2
            // 
            splitContainerBody.Panel2.Controls.Add(comboBoxInput);
            splitContainerBody.Panel2.Controls.Add(buttonExecute);
            splitContainerBody.Size = new System.Drawing.Size(955, 501);
            splitContainerBody.SplitterDistance = 470;
            splitContainerBody.TabIndex = 0;
            // 
            // textBoxOutput
            // 
            textBoxOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            textBoxOutput.Location = new System.Drawing.Point(0, 0);
            textBoxOutput.Multiline = true;
            textBoxOutput.Name = "textBoxOutput";
            textBoxOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            textBoxOutput.Size = new System.Drawing.Size(955, 470);
            textBoxOutput.TabIndex = 2;
            textBoxOutput.WordWrap = false;
            // 
            // buttonExecute
            // 
            buttonExecute.Dock = System.Windows.Forms.DockStyle.Right;
            buttonExecute.Location = new System.Drawing.Point(880, 0);
            buttonExecute.Name = "buttonExecute";
            buttonExecute.Size = new System.Drawing.Size(75, 27);
            buttonExecute.TabIndex = 1;
            buttonExecute.Text = "Execute";
            buttonExecute.UseVisualStyleBackColor = true;
            buttonExecute.Click += ButtonExecute_Click;
            // 
            // comboBoxInput
            // 
            comboBoxInput.Dock = System.Windows.Forms.DockStyle.Fill;
            comboBoxInput.FormattingEnabled = true;
            comboBoxInput.Location = new System.Drawing.Point(0, 0);
            comboBoxInput.Name = "comboBoxInput";
            comboBoxInput.Size = new System.Drawing.Size(880, 23);
            comboBoxInput.TabIndex = 2;
            // 
            // FormDebug
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(955, 501);
            Controls.Add(splitContainerBody);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "FormDebug";
            Text = "Nebula Siege : Debug";
            Load += FormDebug_Load;
            splitContainerBody.Panel1.ResumeLayout(false);
            splitContainerBody.Panel1.PerformLayout();
            splitContainerBody.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerBody).EndInit();
            splitContainerBody.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainerBody;
        private System.Windows.Forms.TextBox textBoxOutput;
        private System.Windows.Forms.Button buttonExecute;
        private System.Windows.Forms.ComboBox comboBoxInput;
    }
}