// File: wwwroot/js/postDelete.js
document.addEventListener('DOMContentLoaded', function() {
    document.querySelectorAll('.delete-post').forEach(button => {
        button.addEventListener('click', function() {
            if (confirm('Are you sure you want to delete this post?')) {
                const postId = this.getAttribute('data-post-id');

                fetch(`/DeletePost/${postId}`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    }
                })
                .then(response => {
                    if (response.ok) {
                        // Remove the post card from the UI
                        const postCard = document.getElementById(postId);
                        if (postCard) {
                            postCard.remove();
                        }
                    } else {
                        return response.json().then(data => {
                            throw new Error(data.message || 'Failed to delete post');
                        });
                    }
                })
                .catch(error => {
                    alert(`Error: ${error.message}`);
                });
            }
        });
    });
});