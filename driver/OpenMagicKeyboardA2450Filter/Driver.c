/*
 * Driver.c — KMDF driver entry point for A2450 HID Filter Driver.
 *
 * Design / build skeleton only.
 * Do not install.  Do not bind to real hardware.
 *
 * Driver model: HIDClass lower filter driver
 * Framework: KMDF
 * Target device: Apple Magic Keyboard A2450 (VID_05AC & PID_029C)
 *
 * Driver stack position:
 *   kbdhid.sys (HID keyboard miniport)
 *       |
 *   [OpenMagicKeyboardA2450Filter.sys]  <- this driver
 *       |
 *   hidusb.sys (HID USB miniport)
 */

#include <ntddk.h>
#include <wdf.h>
#include "Device.h"
#include "ReportTransform.h"
#include "A2450Report.h"

/* Forward declarations */
DRIVER_INITIALIZE DriverEntry;
EVT_WDF_DRIVER_DEVICE_ADD A2450FilterEvtDeviceAdd;
EVT_WDF_IO_QUEUE_IO_INTERNAL_DEVICE_CONTROL A2450FilterEvtIoInternalDeviceControl;

/*
 * DriverEntry — called when the driver is loaded.
 */
NTSTATUS
DriverEntry(
    _In_ PDRIVER_OBJECT  DriverObject,
    _In_ PUNICODE_STRING RegistryPath
)
{
    WDF_DRIVER_CONFIG config;
    NTSTATUS status;

    KdPrint(("OpenMagicKeyboard: DriverEntry\n"));

    WDF_DRIVER_CONFIG_INIT(&config, A2450FilterEvtDeviceAdd);

    status = WdfDriverCreate(
        DriverObject,
        RegistryPath,
        WDF_NO_OBJECT_ATTRIBUTES,
        &config,
        WDF_NO_HANDLE
    );

    if (!NT_SUCCESS(status))
    {
        KdPrint(("OpenMagicKeyboard: WdfDriverCreate failed 0x%08X\n", status));
    }

    return status;
}

/*
 * A2450FilterEvtDeviceAdd — called when PnP manager discovers a matching device.
 *
 * As a HID lower filter:
 *   1. Mark ourselves as a filter driver
 *   2. Create the device object
 *   3. Get the default I/O target (next-lower driver)
 *   4. Create a default queue with EvtIoInternalDeviceControl
 */
NTSTATUS
A2450FilterEvtDeviceAdd(
    _In_ WDFDRIVER   Driver,
    _Inout_ PWDFDEVICE_INIT DeviceInit
)
{
    NTSTATUS status;
    WDFDEVICE device;
    WDF_OBJECT_ATTRIBUTES deviceAttributes;
    WDF_IO_QUEUE_CONFIG queueConfig;
    PA2450_DEVICE_CONTEXT ctx;

    UNREFERENCED_PARAMETER(Driver);

    KdPrint(("OpenMagicKeyboard: DeviceAdd\n"));

    /* Mark as filter driver — must be called before WdfDeviceCreate */
    WdfFdoInitSetFilter(DeviceInit);

    /* Create device object with context */
    WDF_OBJECT_ATTRIBUTES_INIT_CONTEXT_TYPE(&deviceAttributes, A2450_DEVICE_CONTEXT);

    status = WdfDeviceCreate(&DeviceInit, &deviceAttributes, &device);
    if (!NT_SUCCESS(status))
    {
        KdPrint(("OpenMagicKeyboard: WdfDeviceCreate failed 0x%08X\n", status));
        return status;
    }

    /* Initialize device context */
    ctx = A2450GetDeviceContext(device);
    ctx->Device = device;
    ctx->IoTarget = WdfDeviceGetIoTarget(device);
    A2450TransformStateInit(&ctx->TransformState);
    ctx->ReportsTransformed = 0;
    ctx->ReportsPassedThrough = 0;

    /* Create default queue for internal device control (HID IOCTLs) */
    WDF_IO_QUEUE_CONFIG_INIT_DEFAULT_QUEUE(
        &queueConfig,
        WdfIoQueueDispatchParallel
    );

    queueConfig.EvtIoInternalDeviceControl = A2450FilterEvtIoInternalDeviceControl;

    status = WdfIoQueueCreate(
        device,
        &queueConfig,
        WDF_NO_OBJECT_ATTRIBUTES,
        WDF_NO_HANDLE
    );

    if (!NT_SUCCESS(status))
    {
        KdPrint(("OpenMagicKeyboard: WdfIoQueueCreate failed 0x%08X\n", status));
        return status;
    }

    KdPrint(("OpenMagicKeyboard: DeviceAdd success, IoTarget=%p\n", ctx->IoTarget));

    return STATUS_SUCCESS;
}
