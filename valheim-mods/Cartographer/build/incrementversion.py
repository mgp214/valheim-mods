import io

path = 'valheim-mods\\Cartographer\\Plugin.cs'

with open(path) as plugin_cs:
    lines = plugin_cs.readlines()

i = 0
for line in lines:
    if not line.find('pluginVersion = ') == -1:
        version_start = line.find('"') + 1
        version_end = line.rfind('"')
        version = line[version_start:version_end]
        build = version.split('.')[-1]
        build = int(build)
        build += 1
        version = version.split('.')
        version[-1] = str(build)
        version = '.'.join(version)
        print(f'incremented version to {version}')
        new_version_line = line[0:version_start] + version + line[version_end:]
        break
    i += 1

lines[i] = new_version_line

with open(path, 'w') as plugin_cs:
    plugin_cs.writelines(lines)
