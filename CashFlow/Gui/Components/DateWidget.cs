﻿using Dalamud.Interface.ImGuiNotification;
using Dalamud.Interface.Utility.Raii;
using System.Globalization;

namespace CashFlow.Gui.Components;
public static class DateWidget
{
    private const int HeightInItems = 1 + 1 + 1 + 4 + 1;
    private static readonly DateTime Sample = DateTime.UnixEpoch;

    private static readonly Vector4 Transparent = new(1, 1, 1, 0);
    private static readonly string[] DayNames = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];
    private static readonly string[] MonthNames = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
    private static readonly int[] NumDaysPerMonth = [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31];

    private static float LongestMonthWidth;
    private static readonly float[] MonthWidths = new float[12];

    private static uint LastOpenComboID;

    public static bool Validate(DateTime minimal, ref DateTime currentMin, ref DateTime currentMax)
    {
        var needsRefresh = false;
        if(minimal > currentMin)
        {
            currentMin = minimal;
            Svc.NotificationManager.AddNotification(new Notification
            {
                Content = $"Date before {minimal.ToShortDateString()} is not possible",
                Type = NotificationType.Warning,
                Minimized = false,
            });
            needsRefresh = true;
        }
        else if(currentMin > currentMax)
        {
            currentMax = currentMin;
            needsRefresh = true;
        }

        return needsRefresh;
    }

    public static string DateFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern + " " + CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;
    public static bool DatePickerWithInput(string label, int id, ref string dateString, ref DateTime date, out bool isOpen)
    {
        isOpen = false;
        var ret = false;
        var format = DateFormat;
        ImGui.SetNextItemWidth(ImGui.CalcTextSize(Sample.ToString(format)).X + ImGui.GetStyle().ItemInnerSpacing.X * 2);
        if(ImGui.InputTextWithHint($"##{label}Input", format.ToUpper(), ref dateString, 32, ImGuiInputTextFlags.CallbackCompletion))
        {
            if(DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var tmp))
            {
                date = tmp;
            }
        }
        if(ImGui.IsItemDeactivatedAfterEdit())
        {
            ret = true;
        }

        ImGui.SameLine(0, 3.0f * ImGuiHelpers.GlobalScale);

        ImGuiEx.IconButton(FontAwesomeIcon.Calendar, id.ToString());
        if(DatePicker(label, ref date, false, isOpen: out isOpen))
        {
            ret = true;
            dateString = date.ToString(format);
        }
        return ret;
    }

    private static bool DatePicker(string label, ref DateTime dateOut, bool closeWhenMouseLeavesIt, out bool isOpen, string leftArrow = "", string rightArrow = "")
    {
        isOpen = false;
        using var mono = ImRaii.PushFont(UiBuilder.MonoFont);
        if(LongestMonthWidth == 0.0f)
        {
            for(var i = 0; i < 12; i++)
            {
                var mw = ImGui.CalcTextSize(MonthNames[i]).X;

                MonthWidths[i] = mw;
                LongestMonthWidth = Math.Max(LongestMonthWidth, mw);
            }
        }

        var id = ImGui.GetID(label);
        var style = ImGui.GetStyle();

        var arrowLeft = leftArrow.Length > 0 ? leftArrow : "<";
        var arrowRight = rightArrow.Length > 0 ? rightArrow : ">";
        var arrowLeftWidth = ImGui.CalcTextSize(arrowLeft).X;
        var arrowRightWidth = ImGui.CalcTextSize(arrowRight).X;

        var labelSize = ImGui.CalcTextSize(label, true, 0);

        var widthRequiredByCalendar = (2.0f * arrowLeftWidth) + (2.0f * arrowRightWidth) + LongestMonthWidth + ImGui.CalcTextSize("9999").X + (120.0f * ImGuiHelpers.GlobalScale);
        var popupHeight = ((labelSize.Y + (2 * style.ItemSpacing.Y)) * HeightInItems) + (style.FramePadding.Y * 3);

        var valueChanged = false;
        ImGui.SetNextWindowSize(new Vector2(widthRequiredByCalendar, widthRequiredByCalendar));
        ImGui.SetNextWindowSizeConstraints(new Vector2(widthRequiredByCalendar, popupHeight + 40), new Vector2(widthRequiredByCalendar, popupHeight + 40));

        using var popupItem = ImRaii.ContextPopupItem(label, ImGuiPopupFlags.None);
        if(!popupItem.Success)
            return valueChanged;
        isOpen = true;

        if(ImGui.GetIO().MouseClicked[1])
        {
            // reset date when user right-clicks the date chooser header when the dialog is open
            dateOut = DateTime.Now;
        }
        else if(LastOpenComboID != id)
        {
            LastOpenComboID = id;
            if(dateOut.Year == 1)
                dateOut = DateTime.Now;
        }

        using var windowPadding = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, style.FramePadding);
        using var buttonColor = ImRaii.PushColor(ImGuiCol.Button, Transparent);

        ImGui.Spacing();

        var yearString = $"{dateOut.Year}";
        var yearPartWidth = arrowLeftWidth + arrowRightWidth + ImGui.CalcTextSize(yearString).X;

        var oldWindowRounding = style.WindowRounding;
        style.WindowRounding = 0;

        using(ImRaii.PushId(1234))
        {
            if(ImGui.SmallButton(arrowLeft))
            {
                valueChanged = true;
                dateOut = dateOut.AddMonths(-1);
            }

            ImGui.SameLine();

            var color = ImGui.GetColorU32(style.Colors[(int)ImGuiCol.Text]);
            var monthWidth = MonthWidths[dateOut.Month - 1];
            var pos = ImGui.GetCursorScreenPos();
            pos = pos with { X = pos.X + ((LongestMonthWidth - monthWidth) * 0.5f) };

            ImGui.GetForegroundDrawList().AddText(pos, color, MonthNames[dateOut.Month - 1]);

            ImGui.SameLine(0, LongestMonthWidth + style.ItemSpacing.X * 2);

            if(ImGui.SmallButton(arrowRight))
            {
                valueChanged = true;
                dateOut = dateOut.AddMonths(1);
            }
        }

        ImGui.SameLine(ImGui.GetWindowWidth() - yearPartWidth - style.WindowPadding.X - style.ItemSpacing.X * 4.0f);

        using(ImRaii.PushId(1235))
        {
            if(ImGui.SmallButton(arrowLeft))
            {
                valueChanged = true;
                dateOut = dateOut.AddYears(-1);
            }

            ImGui.SameLine();
            ImGui.Text($"{dateOut.Year}");
            ImGui.SameLine();

            if(ImGui.SmallButton(arrowRight))
            {
                valueChanged = true;
                dateOut = dateOut.AddYears(1);
            }
        }

        ImGui.Spacing();

        // This could be calculated only when needed (but I guess it's fast in any case...)
        var maxDayOfCurMonth = NumDaysPerMonth[dateOut.Month - 1];
        if(maxDayOfCurMonth == 28)
        {
            var year = dateOut.Year;
            var bis = ((year % 4) == 0) && ((year % 100) != 0 || (year % 400) == 0);
            if(bis)
                maxDayOfCurMonth = 29;
        }

        using var buttonHovered = ImRaii.PushColor(ImGuiCol.ButtonHovered, ImGuiColors.DalamudOrange);
        using var buttonActive = ImRaii.PushColor(ImGuiCol.ButtonActive, ImGuiColors.DalamudYellow);

        ImGui.Separator();

        // Display items
        var dayClicked = false;
        var dayOfWeek = (int)new DateTime(dateOut.Year, dateOut.Month, 1).NormalDayOfWeek();
        for(var dw = 0; dw < 7; dw++)
        {
            using(ImRaii.Group())
            {
                using var textColor = ImRaii.PushColor(ImGuiCol.Text, CalculateTextColor(), dw == 0);

                ImGui.Text($"{(dw == 0 ? "" : " ")}{DayNames[dw]}");
                if(dw == 0)
                    ImGui.Separator();
                else
                    ImGui.Spacing();

                // Use dayOfWeek for spacing
                var curDay = dw - dayOfWeek;
                for(var row = 0; row < 7; row++)
                {
                    var cday = curDay + (7 * row);
                    if(cday >= 0 && cday < maxDayOfCurMonth)
                    {
                        using var rowId = ImRaii.PushId(row * 10 + dw);
                        if(ImGui.SmallButton(string.Format(cday < 9 ? " {0}" : "{0}", cday + 1)))
                        {
                            ImGui.SetItemDefaultFocus();

                            valueChanged = true;
                            dateOut = new DateTime(dateOut.Year, dateOut.Month, cday + 1, dateOut.Hour, dateOut.Minute, dateOut.Second);
                        }
                    }
                    else
                    {
                        ImGui.TextUnformatted(" ");
                    }
                }

                if(dw == 0)
                    ImGui.Separator();
            }

            if(dw != 6)
                ImGui.SameLine(ImGui.GetWindowWidth() - (6 - dw) * (ImGui.GetWindowWidth() / 7.0f));
        }

        if(ImGui.GetCursorPos().X > ImGui.GetContentRegionMax().X / 2) ImGui.NewLine();

        var hours = dateOut.Hour;
        var minutes = dateOut.Minute;
        ImGui.SetNextItemWidth(ImGui.CalcTextSize("000").X + ImGui.GetFrameHeightWithSpacing() * 2);
        if(ImGui.InputInt("##dtHours", ref hours))
        {
            hours = Math.Clamp(hours, 0, 23);
            valueChanged = true;
            dateOut = new DateTime(dateOut.Year, dateOut.Month, dateOut.Day, hours, dateOut.Minute, dateOut.Second);
        }
        ImGui.SameLine(0, 1);
        ImGuiEx.TextV(":");
        ImGui.SameLine(0, 1);
        ImGui.SetNextItemWidth(ImGui.CalcTextSize("000").X + ImGui.GetFrameHeightWithSpacing() * 2);
        if(ImGui.InputInt("##dtMinutes", ref minutes))
        {
            hours = Math.Clamp(minutes, 0, 23);
            valueChanged = true;
            dateOut = new DateTime(dateOut.Year, dateOut.Month, dateOut.Day, dateOut.Hour, minutes, dateOut.Second);
        }

        style.WindowRounding = oldWindowRounding;

        var mustCloseCombo = dayClicked;
        if(closeWhenMouseLeavesIt && !mustCloseCombo)
        {
            var distance = ImGui.GetFontSize() * 1.75f; //1.3334f; //24;
            var pos = ImGui.GetWindowPos();
            pos.X -= distance;
            pos.Y -= distance;
            var size = ImGui.GetWindowSize();
            size.X += 2.0f * distance;
            size.Y += 2.0f * distance;
            var mousePos = ImGui.GetIO().MousePos;
            if(mousePos.X < pos.X || mousePos.Y < pos.Y || mousePos.X > pos.X + size.X || mousePos.Y > pos.Y + size.Y)
                mustCloseCombo = true;
        }

        // ImGui issue #273849, children keep popups from closing automatically
        if(mustCloseCombo)
            ImGui.CloseCurrentPopup();

        return valueChanged;
    }

    private static Vector4 CalculateTextColor()
    {
        var textColor = ImGuiColors.DalamudGrey;
        var l = (textColor.X + textColor.Y + textColor.Z) * 0.33334f;
        return new Vector4(l * 2.0f > 1 ? 1 : l * 2.0f, l * .5f, l * .5f, textColor.W);
    }

    public static NormalDayOfWeek NormalDayOfWeek(this DateTime date)
    {
        var ndow = (int)date.DayOfWeek - 1;
        if(ndow == -1) ndow = 6;
        return (NormalDayOfWeek)ndow;
    }
}
