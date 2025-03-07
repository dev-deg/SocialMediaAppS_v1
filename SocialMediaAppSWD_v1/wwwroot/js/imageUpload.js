document.addEventListener('DOMContentLoaded', function() {
    const uploadButton = document.getElementById('uploadButton');
    const imageUpload = document.getElementById('imageUpload');
    const imagePreview = document.getElementById('imagePreview');
    const imagePreviewContainer = document.getElementById('imagePreviewContainer');
    const removeImageBtn = document.getElementById('removeImageBtn');
    const postForm = document.getElementById('postForm');
    const postImageUrlInput = document.getElementById('PostImageUrl');
    const submitPostBtn = document.getElementById('submitPostBtn');

    // Trigger file input when the button is clicked
    uploadButton.addEventListener('click', function() {
        imageUpload.click();
    });

    // Handle the file selection
    imageUpload.addEventListener('change', function() {
        if (this.files && this.files[0]) {
            const file = this.files[0];
            // Show loading state
            uploadButton.disabled = true;
            uploadButton.textContent = 'Uploading...';
            
            // Create FormData and append the file
            const formData = new FormData();
            formData.append('image', file);
            
            // Upload the image
            fetch('/UploadImage', {
                method: 'POST',
                body: formData
            })
            .then(response => {
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                }
                return response.json();
            })
            .then(data => {
                // Display the image preview
                imagePreview.src = data.imageUrl;
                imagePreviewContainer.classList.remove('d-none');
                
                // Store the image URL in the hidden input
                postImageUrlInput.value = data.imageUrl;
                
                // Reset the upload button
                uploadButton.textContent = 'Change Image';
                uploadButton.disabled = false;
            })
            .catch(error => {
                console.error('Error:', error);
                alert('Failed to upload image. Please try again.');
                uploadButton.textContent = 'Select Image';
                uploadButton.disabled = false;
            });
        }
    });

    // Remove the image
    removeImageBtn.addEventListener('click', function() {
        imagePreview.src = '';
        imagePreviewContainer.classList.add('d-none');
        postImageUrlInput.value = '';
        imageUpload.value = '';
        uploadButton.textContent = 'Select Image';
    });
});