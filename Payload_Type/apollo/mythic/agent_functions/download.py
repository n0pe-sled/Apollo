from mythic_payloadtype_container.MythicCommandBase import *
import json


class DownloadArguments(TaskArguments):

    def __init__(self, command_line):
        super().__init__(command_line)
        self.args = {
            "file": CommandParameter(name="File", type=ParameterType.String, description="File to download."),
            "host": CommandParameter(name="Host", type=ParameterType.String, description="File to download.", required=False),
        }

    async def parse_arguments(self):
        if len(self.command_line) == 0:
            raise Exception("Require a path to download.\n\tUsage: {}".format(DownloadCommand.help_cmd))
        filename = ""
        if self.command_line[0] == '"' and self.command_line[-1] == '"':
            self.command_line = self.command_line[1:-1]
            filename = self.command_line
        elif self.command_line[0] == "'" and self.command_line[-1] == "'":
            self.command_line = self.command_line[1:-1]
            filename = self.command_line
        elif self.command_line[0] == "{":
            args = json.loads(self.command_line)
            if args.get("path") != None and args.get("file") != None:
                # Then this is a filebrowser thing
                if args["path"][-1] == "\\":
                    self.args["file"].value = args["path"] + args["file"]
                else:
                    self.args["file"].value = args["path"] + "\\" + args["file"]
                self.args["host"].value = args["host"] 
            else:
                # got a modal popup
                self.load_args_from_json_string(self.command_line)
        else:
            filename = self.command_line

        if filename != "":
            if filename[:2] == "\\\\":
                # UNC path
                filename_parts = filename.split("\\")
                if len(filename_parts) < 4:
                    raise Exception("Illegal UNC path or no file could be parsed from: {}".format(filename))
                self.args["host"].value = filename_parts[2]
                self.args["file"].value = "\\".join(filename_parts[3:])
            else:
                self.args["file"].value = filename
                self.args["host"].value = ""



class DownloadCommand(CommandBase):
    cmd = "download"
    needs_admin = False
    help_cmd = "download [path/to/file]"
    description = "Download a file off the target system."
    version = 2
    is_exit = False
    is_file_browse = False
    is_process_list = False
    supported_ui_features = ["file_browser:download"]
    is_upload_file = False
    is_remove_file = False
    author = "@djhohnstein"
    argument_class = DownloadArguments
    attackmapping = ["T1020", "T1030", "T1041"]
    browser_script = BrowserScript(script_name="download_new", author="@djhohnstein", for_new_ui=True)

    async def create_tasking(self, task: MythicTask) -> MythicTask:
        if task.args.get_arg("host"):
            task.display_params = "\\\\{}\\{}".format(task.args.get_arg("host"), task.args.get_arg("file"))
        else:
            task.display_params = task.args.get_arg("file")
        return task

    async def process_response(self, response: AgentResponse):
        pass