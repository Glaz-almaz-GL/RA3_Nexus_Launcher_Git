using System;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using System.Collections.Generic;
using System.Text;

namespace RA3_Nexus_Launcher.Helpers
{
    public static class NotificationHelpers
    {
        private static WindowNotificationManager? _notificationManager;

        /// <summary>
        /// Устанавливает окно, к которому будут привязаны уведомления.
        /// Этот метод должен быть вызван один раз, обычно в конструкторе главного окна.
        /// </summary>
        /// <param name="window">Главное окно приложения.</param>
        public static void SetNotificationManager(Window window)
        {
            // Создаем WindowNotificationManager, привязанный к переданному окну
            _notificationManager = new WindowNotificationManager(window)
            {
                Position = NotificationPosition.BottomRight, // Устанавливаем позицию уведомлений
                MaxItems = 5 // Максимальное количество одновременно отображаемых уведомлений
            };
        }

        /// <summary>
        /// Показывает информационное уведомление.
        /// </summary>
        /// <param name="title">Заголовок уведомления.</param>
        /// <param name="message">Текст уведомления.</param>
        /// <param name="onClick">Действие, выполняемое при клике на уведомление (опционально).</param>
        /// <param name="onClose">Действие, выполняемое при закрытии уведомления (опционально).</param>
        public static void ShowInformation(string title, string message, TimeSpan? expiration = null, Action? onClick = null, Action? onClose = null)
        {
            _notificationManager?.Show(new Notification(title, message, NotificationType.Information, expiration, onClick, onClose));
        }

        /// <summary>
        /// Показывает уведомление об успехе.
        /// </summary>
        public static void ShowSuccess(string title, string message, TimeSpan? expiration = null, Action? onClick = null, Action? onClose = null)
        {
            _notificationManager?.Show(new Notification(title, message, NotificationType.Success, expiration, onClick, onClose));
        }

        /// <summary>
        /// Показывает предупреждающее уведомление.
        /// </summary>
        public static void ShowWarning(string title, string message, TimeSpan? expiration = null, Action? onClick = null, Action? onClose = null)
        {
            _notificationManager?.Show(new Notification(title, message, NotificationType.Warning, expiration, onClick, onClose));
        }

        /// <summary>
        /// Показывает уведомление об ошибке.
        /// </summary>
        public static void ShowError(string title, string message, TimeSpan? expiration = null, Action? onClick = null, Action? onClose = null)
        {
            _notificationManager?.Show(new Notification(title, message, NotificationType.Error, expiration, onClick, onClose));
        }
    }
}
