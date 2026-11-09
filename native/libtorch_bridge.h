#pragma once

#include <stdint.h>

#if defined(_WIN32)
#if defined(AIKERNEL_LIBTORCH_BRIDGE_EXPORTS)
#define AIKERNEL_EXPORT __declspec(dllexport)
#else
#define AIKERNEL_EXPORT __declspec(dllimport)
#endif
#else
#define AIKERNEL_EXPORT __attribute__((visibility("default")))
#endif

#ifdef __cplusplus
extern "C" {
#endif

typedef struct ForwardResultNative {
  int32_t status;
  int32_t output_token_count;
  int32_t output_token_ids[64];
  int32_t logit_count;
  float logits[4096];
} ForwardResultNative;

typedef enum AIKernelCuda13PassId {
  AIKERNEL_CUDA13_PASS_UNKNOWN = 0,
  AIKERNEL_CUDA13_PASS_COMPUTE_DISPATCH = 1,
  AIKERNEL_CUDA13_PASS_LOAD_MODEL = 2,
  AIKERNEL_CUDA13_PASS_UNLOAD_MODEL = 3,
  AIKERNEL_CUDA13_PASS_FORWARD = 4,
  AIKERNEL_CUDA13_PASS_AISTHESIS = 5,
  AIKERNEL_CUDA13_PASS_SPATIAL_REASONING = 6,
  AIKERNEL_CUDA13_PASS_HUD_COMPOSITE = 7
} AIKernelCuda13PassId;

typedef enum AIKernelCuda13DispatchFailureReason {
  AIKERNEL_CUDA13_FAILURE_NONE = 0,
  AIKERNEL_CUDA13_FAILURE_INVALID_REQUEST_LENGTH = 1,
  AIKERNEL_CUDA13_FAILURE_INVALID_ABI = 2,
  AIKERNEL_CUDA13_FAILURE_UNKNOWN_PASS = 3,
  AIKERNEL_CUDA13_FAILURE_CPU_FALLBACK = 4,
  AIKERNEL_CUDA13_FAILURE_DEVICE_LOST = 5,
  AIKERNEL_CUDA13_FAILURE_DEVICE_UNAVAILABLE = 6,
  AIKERNEL_CUDA13_FAILURE_COMMAND_SUBMISSION_DISABLED = 7
} AIKernelCuda13DispatchFailureReason;

typedef struct AIKernelCuda13DispatchRequestHeader {
  uint32_t abi_version;
  uint32_t header_size;
  uint32_t pass_id;
  uint32_t flags;
  uint64_t frame_index;
  uint64_t sample_ticks;
  uint32_t payload_bytes;
  uint32_t reserved;
} AIKernelCuda13DispatchRequestHeader;

typedef struct AIKernelCuda13DispatchResponseHeader {
  uint32_t abi_version;
  uint32_t header_size;
  uint32_t status;
  uint32_t failure_reason;
  uint64_t frame_index;
  uint64_t sample_ticks;
  uint32_t diagnostics_bytes;
  uint32_t reserved;
} AIKernelCuda13DispatchResponseHeader;

AIKERNEL_EXPORT int32_t load_model(const char* path);
AIKERNEL_EXPORT int32_t unload_model(int32_t handle);
AIKERNEL_EXPORT int32_t forward(
    int32_t handle,
    const int32_t* input_ids,
    int32_t length,
    ForwardResultNative* out_result);
AIKERNEL_EXPORT uint32_t aikernel_cuda13_dispatch(
    const void* request,
    uint32_t request_length,
    void* response,
    uint32_t response_length);

#ifdef __cplusplus
}
#endif
