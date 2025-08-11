
Render Pass Configuration

The render pass must describe both the primary OpenXR swapchain color attachment and the additional color attachment, along with the existing depth attachment.

VkAttachmentDescription attachments[3] = {};

// Primary color (OpenXR swapchain)
attachments[0].format = primaryColorFormat;
attachments[0].samples = sampleCount;
attachments[0].loadOp = VK_ATTACHMENT_LOAD_OP_CLEAR;
attachments[0].storeOp = VK_ATTACHMENT_STORE_OP_STORE;
attachments[0].initialLayout = VK_IMAGE_LAYOUT_UNDEFINED;
attachments[0].finalLayout = VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL;

// Secondary color
attachments[1].format = secondaryColorFormat;
attachments[1].samples = sampleCount;
attachments[1].loadOp = VK_ATTACHMENT_LOAD_OP_CLEAR;
attachments[1].storeOp = VK_ATTACHMENT_STORE_OP_STORE;
attachments[1].initialLayout = VK_IMAGE_LAYOUT_UNDEFINED;
attachments[1].finalLayout = VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL;

// Depth
attachments[2].format = depthFormat;
attachments[2].samples = sampleCount;
attachments[2].loadOp = VK_ATTACHMENT_LOAD_OP_CLEAR;
attachments[2].storeOp = VK_ATTACHMENT_STORE_OP_DONT_CARE;
attachments[2].stencilLoadOp = VK_ATTACHMENT_LOAD_OP_DONT_CARE;
attachments[2].stencilStoreOp = VK_ATTACHMENT_STORE_OP_DONT_CARE;
attachments[2].initialLayout = VK_IMAGE_LAYOUT_UNDEFINED;
attachments[2].finalLayout = VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL;

VkAttachmentReference colorRefs[2] = {
    {0, VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL},
    {1, VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL}
};

VkAttachmentReference depthRef{2, VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL};

VkSubpassDescription subpass{};
subpass.pipelineBindPoint = VK_PIPELINE_BIND_POINT_GRAPHICS;
subpass.colorAttachmentCount = 2;
subpass.pColorAttachments = colorRefs;
subpass.pDepthStencilAttachment = &depthRef;

VkRenderPassCreateInfo renderPassInfo{VK_STRUCTURE_TYPE_RENDER_PASS_CREATE_INFO};
renderPassInfo.attachmentCount = 3;
renderPassInfo.pAttachments = attachments;
renderPassInfo.subpassCount = 1;
renderPassInfo.pSubpasses = &subpass;

vkCreateRenderPass(device, &renderPassInfo, nullptr, &renderPass);


⸻

Pipeline Color Blend State

The graphics pipeline must include a blend state entry for each color attachment in the subpass.

VkPipelineColorBlendAttachmentState blendStates[2]{};

for (uint32_t i = 0; i < 2; ++i) {
    blendStates[i].blendEnable = VK_FALSE;
    blendStates[i].colorWriteMask =
        VK_COLOR_COMPONENT_R_BIT |
        VK_COLOR_COMPONENT_G_BIT |
        VK_COLOR_COMPONENT_B_BIT |
        VK_COLOR_COMPONENT_A_BIT;
}

VkPipelineColorBlendStateCreateInfo blendInfo{VK_STRUCTURE_TYPE_PIPELINE_COLOR_BLEND_STATE_CREATE_INFO};
blendInfo.attachmentCount = 2;
blendInfo.pAttachments = blendStates;

The VkGraphicsPipelineCreateInfo must reference this blendInfo and be compatible with the updated render pass.

⸻

Framebuffer Creation

Each framebuffer must contain image views for the primary color attachment, the secondary color attachment, and the depth attachment.

std::array<VkImageView, 3> framebufferAttachments = {
    primaryColorView,
    secondaryColorView,
    depthView
};

VkFramebufferCreateInfo framebufferInfo{VK_STRUCTURE_TYPE_FRAMEBUFFER_CREATE_INFO};
framebufferInfo.renderPass = renderPass;
framebufferInfo.attachmentCount = static_cast<uint32_t>(framebufferAttachments.size());
framebufferInfo.pAttachments = framebufferAttachments.data();
framebufferInfo.width = renderWidth;
framebufferInfo.height = renderHeight;
framebufferInfo.layers = layerCount;

vkCreateFramebuffer(device, &framebufferInfo, nullptr, &framebuffer);


⸻

Command Buffer Recording

The render pass must be begun with clear values for both color attachments and the depth attachment.

VkClearValue clearValues[3];
clearValues[0].color = {{0.0f, 0.0f, 0.0f, 1.0f}}; // Primary color
clearValues[1].color = {{0.0f, 0.0f, 0.0f, 1.0f}}; // Secondary color
clearValues[2].depthStencil = {1.0f, 0};

VkRenderPassBeginInfo beginInfo{VK_STRUCTURE_TYPE_RENDER_PASS_BEGIN_INFO};
beginInfo.renderPass = renderPass;
beginInfo.framebuffer = framebuffer;
beginInfo.renderArea.offset = {0, 0};
beginInfo.renderArea.extent = {renderWidth, renderHeight};
beginInfo.clearValueCount = 3;
beginInfo.pClearValues = clearValues;

vkCmdBeginRenderPass(commandBuffer, &beginInfo, VK_SUBPASS_CONTENTS_INLINE);

// Bind pipeline and issue draw calls here.

vkCmdEndRenderPass(commandBuffer);


⸻

Shader Output

If the secondary color attachment should receive unique output, the fragment shader must declare and write to a second output variable.

layout(location = 0) out vec4 outPrimaryColor;
layout(location = 1) out vec4 outSecondaryColor;

void main() {
    outPrimaryColor = vec4(1.0, 0.0, 0.0, 1.0); // Red
    outSecondaryColor = vec4(0.0, 1.0, 0.0, 1.0); // Green
}


⸻