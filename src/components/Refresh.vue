<template>
<div id="refresher">
  <!-- Spinner on page reload -->
  <b-spinner id="load" label="Loading..."></b-spinner>
  <!-- Reload button -->
  <b-button class="refresh" id="rb" @click="refreshGraph" variant="info">&#8635;</b-button>
  <!-- Reload button spinner -->
  <b-button class="refresh" id="rs" variant="info" disabled>
    <b-spinner small style="margin-bottom: 2px"></b-spinner>
    <span class="sr-only">Loading...</span>
  </b-button>
</div>
</template>

<script>
export default {
name: "Refresh",
  methods: {
    displayRefreshSpinner (b) { // Used when clicking the Refresh button
      const spin = document.getElementById("rs")
      const button = document.getElementById("rb")
      if (b) {
        spin.style.display = "block"
        button.style.display = "none"
      } else {
        spin.style.display = "none"
        button.style.display = "block"
      }
    },
    displayReloadSpinner (b) { // Used when reloading the page (F5)
      const spinner = document.getElementById("load")
      const refresh = document.getElementById("rb")
      if (b) {
        spinner.style.display = "block"
        refresh.style.display = "none"
        return false // Tells App whether the Statusbar should be shown
      } else {
        spinner.style.display = "none"
        refresh.style.display = "block"
        return true
      }
    },
    refreshGraph () {
      this.$emit('button-click')
    }
  }
}
</script>

<style scoped>
.refresh {
  position: fixed;
  bottom: 70px;
  left: 20px;
}

#rs {
  display: none;
}

#load {
  position: fixed;
  left: 48%;
  bottom: 48%;
  width: 3rem;
  height: 3rem;
}
</style>
