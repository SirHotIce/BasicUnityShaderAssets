# Adding a Second Color Attachment in hello_xr (Vulkan Path)

This document describes the modifications required to integrate a second color attachment in the Vulkan rendering path of `hello_xr`. It assumes that the OpenXR side already provides the secondary swapchain or image.

---

## Render Pass Changes

Locate the render pass creation (in `graphicsplugin_vulkan.cpp`) and expand it to include a second color attachment.

```cpp
// Attachments: [0]=primary XR color, [1]=secondary color, [2]=depth
VkAttachmentDescription atts[3] = {};

// primary color (XR swapchain format)
atts[0].format        = primaryColorFormat;
atts[0].samples       = sampleCount;
atts[0].loadOp        = VK_ATTACHMENT_LOAD_OP_CLEAR;
atts[0].storeOp       = VK_ATTACHMENT_STORE_OP_STORE;
atts[0].initialLayout = VK_IMAGE_LAYOUT_UNDEFINED;
atts[0].finalLayout   = VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL;

// secondary color
atts[1].format        = secondaryColorFormat;
atts[1].samples       = sampleCount;
atts[1].loadOp        = VK_ATTACHMENT_LOAD_OP_CLEAR;
atts[1].storeOp       = VK_ATTACHMENT_STORE_OP_STORE;
atts[1].initialLayout = VK_IMAGE_LAYOUT_UNDEFINED;
atts[1].finalLayout   = VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL;

// depth
atts[2].format        = depthFormat;
atts[2].samples       = sampleCount;
atts[2].loadOp        = VK_ATTACHMENT_LOAD_OP_CLEAR;
atts[2].storeOp       = VK_ATTACHMENT_STORE_OP_DONT_CARE;
atts[2].stencilLoadOp = VK_ATTACHMENT_LOAD_OP_DONT_CARE;
atts[2].stencilStoreOp= VK_ATTACHMENT_STORE_OP_DONT_CARE;
atts[2].initialLayout = VK_IMAGE_LAYOUT_UNDEFINED;
atts[2].finalLayout   = VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL;

VkAttachmentReference colorRefs[2] = {
    {0, VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL},
    {1, VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL}
};
VkAttachmentReference depthRef{2, VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL};

VkSubpassDescription subpass{};
subpass.pipelineBindPoint       = VK_PIPELINE_BIND_POINT_GRAPHICS;
subpass.colorAttachmentCount    = 2;
subpass.pColorAttachments       = colorRefs;
subpass.pDepthStencilAttachment = &depthRef;

VkRenderPassCreateInfo rpci{VK_STRUCTURE_TYPE_RENDER_PASS_CREATE_INFO};
rpci.attachmentCount = 3;
rpci.pAttachments    = atts;
rpci.subpassCount    = 1;
rpci.pSubpasses      = &subpass;

VK_CHECK(vkCreateRenderPass(device, &rpci, nullptr, &renderPass));
```

---

## Pipeline Color Blend State

In `CreateGraphicsPipeline(...)`, set the blend state for two color attachments.

```cpp
VkPipelineColorBlendAttachmentState blends[2]{};
for (uint32_t i = 0; i < 2; ++i) {
    blends[i].blendEnable = VK_FALSE;
    blends[i].colorWriteMask =
        VK_COLOR_COMPONENT_R_BIT | VK_COLOR_COMPONENT_G_BIT |
        VK_COLOR_COMPONENT_B_BIT | VK_COLOR_COMPONENT_A_BIT;
}

VkPipelineColorBlendStateCreateInfo cb{VK_STRUCTURE_TYPE_PIPELINE_COLOR_BLEND_STATE_CREATE_INFO};
cb.attachmentCount = 2;
cb.pAttachments    = blends;
```

Update the fragment shader to output to both attachments:

```glsl
layout(location = 0) out vec4 outColor0;
layout(location = 1) out vec4 outColor1;
```

---

## Swapchain Image Views

If using a second XR swapchain, enumerate its images and create `VkImageView`s similar to the primary.  
If using offscreen images, create them with `VK_IMAGE_USAGE_COLOR_ATTACHMENT_BIT` (and `SAMPLED_BIT` if needed).

---

## Framebuffer Creation

When creating framebuffers, attach the primary color, secondary color, and depth image views.

```cpp
std::array<VkImageView, 3> fbViews = {
    primaryColorView,
    secondaryColorView,
    depthView
};

VkFramebufferCreateInfo fbi{VK_STRUCTURE_TYPE_FRAMEBUFFER_CREATE_INFO};
fbi.renderPass      = renderPass;
fbi.attachmentCount = static_cast<uint32_t>(fbViews.size());
fbi.pAttachments    = fbViews.data();
fbi.width           = renderWidth;
fbi.height          = renderHeight;
fbi.layers          = layerCount;

VK_CHECK(vkCreateFramebuffer(device, &fbi, nullptr, &framebuffer));
```

---

## Command Buffer Recording

Extend the clear values array and begin the render pass.

```cpp
VkClearValue clears[3];
clears[0].color        = {{0.f, 0.f, 0.f, 1.f}};
clears[1].color        = {{0.f, 0.f, 0.f, 1.f}};
clears[2].depthStencil = {1.f, 0};

VkRenderPassBeginInfo rpb{VK_STRUCTURE_TYPE_RENDER_PASS_BEGIN_INFO};
rpb.renderPass      = renderPass;
rpb.framebuffer     = framebufferForThisViewAndIndex;
rpb.renderArea      = {{0, 0}, {renderWidth, renderHeight}};
rpb.clearValueCount = 3;
rpb.pClearValues    = clears;

vkCmdBeginRenderPass(cmd, &rpb, VK_SUBPASS_CONTENTS_INLINE);
// draw calls
vkCmdEndRenderPass(cmd);
```

---

## Acquire/Release for Secondary XR Swapchain

If the secondary attachment is an XR swapchain:

```cpp
XrSwapchainImageAcquireInfo acq{XR_TYPE_SWAPCHAIN_IMAGE_ACQUIRE_INFO};
uint32_t idx1 = 0; 
xrAcquireSwapchainImage(secondarySwapchain, &acq, &idx1);

XrSwapchainImageWaitInfo waitInfo{XR_TYPE_SWAPCHAIN_IMAGE_WAIT_INFO};
waitInfo.timeout = XR_INFINITE_DURATION;
xrWaitSwapchainImage(secondarySwapchain, &waitInfo);

// use secondaryColorImageViews[idx1] for framebuffer creation

XrSwapchainImageReleaseInfo rel{XR_TYPE_SWAPCHAIN_IMAGE_RELEASE_INFO};
xrReleaseSwapchainImage(secondarySwapchain, &rel);
```

---

## Notes

- The OpenXR compositor will only present the primary color swapchain.
- Formats, sample counts, and dimensions must match across attachments in the subpass.
- For multiview, ensure `VK_IMAGE_VIEW_TYPE_2D_ARRAY` and matching `arrayLayers` for all attachments.
