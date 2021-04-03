
namespace Service_read_file_parser
{
    partial class Service_read_file
    {
        /// <summary> 
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        /// <summary> 
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.eventLogService = new System.Diagnostics.EventLog();
            ((System.ComponentModel.ISupportInitialize)(this.eventLogService)).BeginInit();
            // 
            // Service_read_file
            // 
            this.ServiceName = "Service_read_file";
            ((System.ComponentModel.ISupportInitialize)(this.eventLogService)).EndInit();

        }

        #endregion

        private System.Diagnostics.EventLog eventLogService;
    }
}
