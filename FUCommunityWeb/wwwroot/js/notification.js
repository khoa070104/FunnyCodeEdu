console.log("Notification.js loaded");

function loadNotifications() {
    console.log("Loading notifications...");
    $.get('/Notification/GetUnreadNotifications', function(notifications) {
        console.log("Received notifications:", notifications);
        const container = $('.notifications-container');
        container.empty();
        
        if (notifications.length === 0) {
            container.append('<div class="dropdown-item text-center">No notifications</div>');
            $('.notification-count').hide();
        } else {
            $('.notification-count').text(notifications.length).show();
            
            notifications.forEach(notification => {
                const timeAgo = moment(notification.createdDate).fromNow();
                container.append(`
                    <a href="/Forum/PostDetail?postId=${notification.postID}" 
                       class="dropdown-item notification-item" 
                       data-notification-id="${notification.notificationID}">
                        <div class="d-flex align-items-center">
                            <img src="${notification.fromUser.avatarImage || '/img/avt2.jpg'}" 
                                 class="rounded-circle" 
                                 width="40" 
                                 height="40">
                            <div class="ms-2">
                                <div class="notification-text">${notification.message}</div>
                                <small class="text-muted">${timeAgo}</small>
                            </div>
                        </div>
                    </a>
                `);
            });
        }
    }).fail(function(error) {
        console.error("Error loading notifications:", error);
    });
}

// Cập nhật thông báo mỗi 10 giây
setInterval(loadNotifications, 10000);

// Đánh dấu đã đọc khi click vào thông báo
$(document).on('click', '.notification-item', function(e) {
    e.preventDefault();
    const notificationId = $(this).data('notification-id');
    const href = $(this).attr('href');
    
    $.post('/Notification/MarkAsRead', { notificationId: notificationId })
        .done(function() {
            window.location.href = href;
        })
        .fail(function(error) {
            console.error("Error marking notification as read:", error);
        });
});

// Load thông báo khi trang được tải
$(document).ready(function() {
    console.log("Document ready, loading notifications...");
    loadNotifications();
});