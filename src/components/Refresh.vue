<template>
<div id="refresher">
  <!-- Reload button -->
  <b-button class="refresh" id="rb" @click="refreshGraph" variant="info">&#8635;</b-button>
  <!-- Reload button spinner -->
  <b-button class="refresh" id="rs" variant="info" disabled>
    <b-spinner small style="margin-bottom: 2px"></b-spinner>
    <span class="sr-only">Loading...</span>
  </b-button>
</div>
</template>

<script lang="ts">
import Component from 'vue-class-component'
import Vue from "vue"

@Component
export default class Refresh extends Vue {

  displayRefreshSpinner (b: boolean) { // Used when clicking the Refresh button
    const spin = document.getElementById("rs")
    const button = document.getElementById("rb")
    if (b) {
      spin!.style.display = "block"
      button!.style.display = "none"
    } else {
      spin!.style.display = "none"
      button!.style.display = "block"
    }
  }

  displayReloadSpinner (b: boolean) { // Used when reloading the page (F5)
    const spinner = document.getElementById("load")
    const refresh = document.getElementById("rb")
    if (b) {
      spinner!.style.display = "block"
      refresh!.style.display = "none"
      return false // Tells App whether the Statusbar should be shown
    } else {
      spinner!.style.display = "none"
      refresh!.style.display = "block"
      return true
    }
  }
  
  refreshGraph () {
    this.$emit('button-click')
  }
  
}
</script>

<style scoped>
.refresh {
  /* position: fixed;
  bottom: 20px;
  left: 1050px; */
}

#rs {
  display: none;
}
</style>
