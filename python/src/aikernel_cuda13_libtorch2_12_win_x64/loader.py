"""[EN]
Reference module for aikernel_cuda13_libtorch2_12_win_x64.loader.

[JA]
aikernel_cuda13_libtorch2_12_win_x64.loader の参照モジュールです。
"""

from __future__ import annotations

import json
import os
from dataclasses import dataclass
from importlib.resources import files
from pathlib import Path
from typing import Any

LOADER_ENV = "AIKERNEL_CUDA13_LIBTORCH2_12_WIN_X64_LOADER"


@dataclass(frozen=True)
class LoaderConfig:
    """[EN]
    Represents the LoaderConfig public Python API surface.
    
    [JA]
    LoaderConfig の公開 Python API サーフェスを表します。
    """

    package_id: str
    rid: str
    native_library: str
    native_abi_path: str
    runtime_search_paths: tuple[str, ...]
    source_path: str

    def resolved_runtime_search_paths(
        self,
        base_path: str | os.PathLike[str] | None = None,
    ) -> tuple[str, ...]:
        """[EN]
        Executes the resolved runtime search paths operation.
        Args:
            base_path: Input value for resolved runtime search paths.
        
        Returns:
            Result produced by the operation.
        
        [JA]
        resolved runtime search paths 操作を実行します。
        引数:
            base_path: resolved runtime search paths に渡す入力値です。
        
        戻り値:
            操作によって生成される結果です。
        """
        root = Path(base_path) if base_path is not None else Path(self.source_path).parent
        resolved: list[str] = []

        for raw_path in self.runtime_search_paths:
            expanded = _expand_percent_env(raw_path)
            if not expanded:
                continue

            candidate = Path(expanded.replace("/", os.sep))
            if not candidate.is_absolute():
                candidate = root / candidate

            resolved.append(str(candidate))

        return tuple(resolved)


def default_loader_json_path() -> str:
    """[EN]
    Executes the default loader json path operation.
    Returns:
        Result produced by the operation.
    
    [JA]
    default loader json path 操作を実行します。
    戻り値:
        操作によって生成される結果です。
    """

    return str(files(__package__).joinpath("loader.json"))


def load_loader_config(
    path: str | os.PathLike[str] | None = None,
) -> LoaderConfig:
    """[EN]
    Executes the load loader config operation.
    Args:
        path: Input value for load loader config.
    
    Returns:
        Result produced by the operation.
    
    [JA]
    load loader config 操作を実行します。
    引数:
        path: load loader config に渡す入力値です。
    
    戻り値:
        操作によって生成される結果です。
    """

    selected = Path(
        path or os.environ.get(LOADER_ENV) or default_loader_json_path()
    )

    with selected.open("r", encoding="utf-8") as handle:
        payload: dict[str, Any] = json.load(handle)

    return LoaderConfig(
        package_id=str(payload.get("packageId") or ""),
        rid=str(payload.get("rid") or ""),
        native_library=str(payload.get("nativeLibrary") or "libtorch_bridge.dll"),
        native_abi_path=str(payload.get("nativeAbiPath") or "runtimes/win-x64/native"),
        runtime_search_paths=tuple(str(value) for value in payload.get("runtimeSearchPaths") or ()),
        source_path=str(selected),
    )


def _expand_percent_env(value: str) -> str:
    expanded = value
    while "%" in expanded:
        start = expanded.find("%")
        end = expanded.find("%", start + 1)
        if end < 0:
            return ""

        name = expanded[start + 1:end]
        replacement = os.environ.get(name)
        if replacement is None:
            return ""

        expanded = expanded[:start] + replacement + expanded[end + 1:]

    return os.path.expandvars(expanded)


__all__ = [
    "LOADER_ENV",
    "LoaderConfig",
    "default_loader_json_path",
    "load_loader_config",
]
