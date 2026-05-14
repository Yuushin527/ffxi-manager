--[[
    ClaudeLogin.lua
    Windower 4 addon

    Automates the POL -> ToS -> FFXI -> character selection login sequence.
    Triggered when autoPOL writes claudelogin_slot.txt next to Windower.exe.

    Author  : autoPOL
    Version : 1.0
--]]

_addon = {}
_addon.name    = 'ClaudeLogin'
_addon.author  = 'autoPOL'
_addon.version = '1.0'

-- ---------------------------------------------------------------------------
-- Constants
-- ---------------------------------------------------------------------------

-- Delay (seconds) to wait for each POL/FFXI screen to fully render before
-- sending keystrokes. Increase these on slow machines if steps are missed.
local DELAY_TOS        = 8    -- seconds after addon loads before pressing Enter on ToS
local DELAY_POL_MENU   = 6    -- seconds after ToS Enter before pressing Enter on FFXI in POL menu
local DELAY_CHAR_SCREEN = 10  -- seconds after game starts before character selection
local DELAY_BETWEEN_MOVES = 0.4  -- seconds between each directional-key press when navigating slots

-- ---------------------------------------------------------------------------
-- Helpers
-- ---------------------------------------------------------------------------

-- Read the first line of a file; returns nil if the file cannot be opened.
local function read_first_line(path)
    local f = io.open(path, 'r')
    if not f then return nil end
    local line = f:read('*l')
    f:close()
    return line
end

-- Return the slot number (1-6) written by autoPOL, or nil if not found / invalid.
local function get_target_slot()
    -- Primary location: same directory as Windower.exe
    local windower_dir = windower.windower_path
    -- windower_path typically ends with a backslash; guard against double-slash.
    if windower_dir:sub(-1) ~= '\\' and windower_dir:sub(-1) ~= '/' then
        windower_dir = windower_dir .. '\\'
    end

    local paths = {
        windower_dir .. 'claudelogin_slot.txt',
    }

    local raw = nil
    for _, p in ipairs(paths) do
        raw = read_first_line(p)
        if raw then
            windower.add_to_chat(8, '[ClaudeLogin] Found slot file: ' .. p)
            break
        end
    end

    if not raw then
        return nil
    end

    local slot = tonumber(raw:match('%d+'))
    if not slot or slot < 1 or slot > 6 then
        windower.add_to_chat(167, '[ClaudeLogin] Invalid slot value: "' .. tostring(raw) .. '". Aborting.')
        return nil
    end

    return slot
end

-- Send a key by name using windower.send_key.
-- key_name : string recognised by Windower (e.g. 'return', 'down', '1' … '6')
-- pressed   : true = key down, false = key up  (send both for a full press)
local function press_key(key_name)
    windower.send_key(key_name, true)   -- key down
    windower.send_key(key_name, false)  -- key up
end

-- ---------------------------------------------------------------------------
-- Login coroutine
-- ---------------------------------------------------------------------------

local function run_login_sequence(slot)
    windower.add_to_chat(8, '[ClaudeLogin] Starting auto-login for slot ' .. slot)

    -- -----------------------------------------------------------------------
    -- Step 1: ToS screen  ("同意する" / Terms of Service)
    -- The screen appears a few seconds after POL is visible.
    -- -----------------------------------------------------------------------
    windower.add_to_chat(8, '[ClaudeLogin] Waiting ' .. DELAY_TOS .. 's for ToS screen...')
    coroutine.sleep(DELAY_TOS)

    windower.add_to_chat(8, '[ClaudeLogin] Pressing Enter to accept ToS.')
    press_key('return')

    -- -----------------------------------------------------------------------
    -- Step 2: POL main menu  (select "Final Fantasy XI")
    -- After accepting ToS, POL shows its top-level menu.
    -- Assumption: FFXI is already highlighted by default, so one Enter suffices.
    -- If your POL setup requires navigation, add 'down' key presses here.
    -- -----------------------------------------------------------------------
    windower.add_to_chat(8, '[ClaudeLogin] Waiting ' .. DELAY_POL_MENU .. 's for POL menu...')
    coroutine.sleep(DELAY_POL_MENU)

    windower.add_to_chat(8, '[ClaudeLogin] Pressing Enter to select FFXI.')
    press_key('return')

    -- -----------------------------------------------------------------------
    -- Step 3: FFXI character selection screen
    -- The game client must load before the character list appears.
    -- -----------------------------------------------------------------------
    windower.add_to_chat(8, '[ClaudeLogin] Waiting ' .. DELAY_CHAR_SCREEN .. 's for character selection screen...')
    coroutine.sleep(DELAY_CHAR_SCREEN)

    -- Navigate to the target slot.
    -- Slot 1 is the default cursor position; pressing Down moves to slot 2, etc.
    local moves = slot - 1
    if moves > 0 then
        windower.add_to_chat(8, '[ClaudeLogin] Moving down ' .. moves .. ' slot(s).')
        for i = 1, moves do
            press_key('down')
            coroutine.sleep(DELAY_BETWEEN_MOVES)
        end
    end

    windower.add_to_chat(8, '[ClaudeLogin] Pressing Enter to select character in slot ' .. slot .. '.')
    press_key('return')

    windower.add_to_chat(8, '[ClaudeLogin] Auto-login sequence complete.')
end

-- ---------------------------------------------------------------------------
-- Addon load event
-- ---------------------------------------------------------------------------

windower.register_event('load', function()
    windower.add_to_chat(8, '[ClaudeLogin] Addon loaded.')

    local slot = get_target_slot()
    if not slot then
        windower.add_to_chat(8, '[ClaudeLogin] No valid slot file found. Running in manual mode.')
        return
    end

    windower.add_to_chat(8, '[ClaudeLogin] Target slot: ' .. slot .. '. Beginning login sequence in background coroutine.')

    -- Run the sequence in a coroutine so the addon loader is not blocked.
    coroutine.wrap(function()
        run_login_sequence(slot)
    end)()
end)

-- ---------------------------------------------------------------------------
-- Unload event (cleanup / informational)
-- ---------------------------------------------------------------------------

windower.register_event('unload', function()
    windower.add_to_chat(8, '[ClaudeLogin] Addon unloaded.')
end)
