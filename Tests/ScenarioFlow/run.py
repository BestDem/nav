"""Run actual scenario/session/controller code against deterministic Unity and HTTP stubs."""
from pathlib import Path
import json, subprocess, tempfile
root = Path(__file__).resolve().parents[2]
unity = Path('/Applications/Unity/Hub/Editor/6000.4.9f1/Unity.app/Contents/Resources/Scripting')
runtime = next((unity / 'NetCoreRuntime/shared/Microsoft.NETCore.App').iterdir())
with tempfile.TemporaryDirectory(prefix='vsm-flow-') as folder:
    folder = Path(folder)
    output = folder / 'tests.dll'
    sources = list((root / 'Tests/ScenarioFlow').glob('*.cs'))
    sources += [root / 'Assets/Scripts' / p for p in (
        'VSM_Unity_WebGL_Dialogue/VSMModels.cs',
        'VSM_Unity_WebGL_Dialogue/VSMGameSession.cs',
        'VSM_Unity_WebGL_Dialogue/PassengerDialogue.cs',
        'GameSettings/People/ControllerPeople.cs',
        'GameSettings/StationController.cs',
        'UI/AssessmentText.cs')]
    rsp = folder / 'compile.rsp'
    rsp.write_text('\n'.join(['-target:exe', '-nologo', f'-out:"{output}"'] +
        [f'-r:"{p}"' for p in runtime.glob('*.dll')] + [f'"{p}"' for p in sources]))
    dotnet = unity / 'NetCoreRuntime/dotnet'
    subprocess.run([str(dotnet), str(unity/'DotNetSdkRoslyn/csc.dll'), '@'+str(rsp)], check=True, cwd=root)
    output.with_suffix('.runtimeconfig.json').write_text(json.dumps({'runtimeOptions': {
        'tfm': 'net8.0', 'framework': {'name':'Microsoft.NETCore.App', 'version':runtime.name}}}))
    subprocess.run([str(dotnet), str(output)], check=True, cwd=root)
